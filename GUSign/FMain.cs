using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Net.Sf.Pkcs11;
using Net.Sf.Pkcs11.Objects;
using Net.Sf.Pkcs11.Wrapper;
using Net.Sf.Pkcs11.EtokenExtensions;

namespace GUSign
{
    public partial class FMain : Form
    {
        public FMain()
        {
            InitializeComponent();
        }
        private string pIN;
        private const string dLLPath = "Resources\\eTPKCS11g.dll";
        private void bCertExport_Click(object sender, EventArgs e)
        {
            // Форму ввода пин-кода я сделал отдельно, чтобы всякий желающий мог ее усилить средствами
            // противодействия перехвату ввода
            FPIN fPIN = new FPIN();
            fPIN.ShowDialog();
            if (fPIN.DialogResult == DialogResult.OK)
            {
                pIN = fPIN.PIN;
                bCertExport.Enabled = false;
                Application.DoEvents();
                try
                {
                    DoWithFirstEtokenSlotWhileLoggedIn(delegate(EtokenModule aEtokenModule, Session aSession)
                    {
                        // Извлекаем сертификат
                        aSession.FindObjectsInit(
                            new P11Attribute[]
                        {
			                new ObjectClassAttribute(CKO.CERTIFICATE)
			            });

                        P11Object[] lObjects = aSession.FindObjects(1);
                        if (lObjects.Length == 0)
                        {
                            MessageBox.Show(this, "Сертификат не найден");
                            return;
                        }
                        // Форматируем его в формате X509
                        X509PublicKeyCertificate certificate = (X509PublicKeyCertificate)(lObjects[0]);
                        aSession.FindObjectsFinal();

                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = "Файлы сертификатов DER (*.cer)|*.cer";
                        
                        saveFileDialog.FilterIndex = 0;
                        saveFileDialog.RestoreDirectory = true;
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            using (BinaryWriter binWriter = new BinaryWriter(File.Open(saveFileDialog.FileName, FileMode.Create)))
                            {
                                binWriter.Write(certificate.Value.Value);
                                MessageBox.Show("Экспорт выполнен");
                            }
                        }
                    });
                }
                finally
                {
                    bCertExport.Enabled = true;
                }
            }
        }
        
        private void bSign_Click(object sender, EventArgs e)
        {
            FPIN fPIN = new FPIN();
            fPIN.ShowDialog();
            if (fPIN.DialogResult == DialogResult.OK)
            {
                pIN = fPIN.PIN;
                bSign.Enabled = false;
                Application.DoEvents();
                try
                {
                    DoWithFirstEtokenSlotWhileLoggedIn(delegate(EtokenModule aEtokenModule, Session aSession)
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "Все файлы (*.*)|*.*";
                        openFileDialog.FilterIndex = 0;
                        openFileDialog.RestoreDirectory = true;
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Находим закрытый ключ
                            aSession.FindObjectsInit(new P11Attribute[]{
			                        	        new ObjectClassAttribute(CKO.PRIVATE_KEY),
			                        	        new KeyTypeAttribute(CKK.GOST)
			                                });
                            P11Object[] lObjects = aSession.FindObjects(1);
                            if (lObjects.Length == 0)
                            {
                                MessageBox.Show(this, "Закрытый ключ не найден");
                                return;
                            }
                            GostPrivateKey pk = lObjects[0] as GostPrivateKey;
                            aSession.FindObjectsFinal();

                            // Находим открытый ключ
                            aSession.FindObjectsInit(new P11Attribute[]{
			                        	        new ObjectClassAttribute(CKO.PUBLIC_KEY),
			                        	        new KeyTypeAttribute(CKK.GOST)
			                                });

                            lObjects = aSession.FindObjects(1);
                            if (lObjects.Length == 0)
                            {
                                MessageBox.Show(this, "Открытый ключ не найден");
                                return;
                            }
                            GostPublicKey pubKey = lObjects[0] as GostPublicKey;
                            aSession.FindObjectsFinal();

                            // Находим сертификат
                            aSession.FindObjectsInit(new P11Attribute[]{
			                                        new ObjectClassAttribute(CKO.CERTIFICATE)
			                                    });
                            lObjects = aSession.FindObjects(1);
                            if (lObjects.Length == 0)
                            {
                                MessageBox.Show(this, "Сертификат не найден");
                                return;
                            }
                            Certificate certificate = lObjects[0] as Certificate;
                            aSession.FindObjectsFinal();

                            FileStream openFileStream = File.Open(openFileDialog.FileName, FileMode.Open);
                            byte[] data = new byte[openFileStream.Length];
                            openFileStream.Read(data, 0, data.Length);
                            byte[] lEnvelope;
                            // Подписываем и упаковываем в конверт
                            aEtokenModule.pkcs7Sign(aSession, data, certificate, out lEnvelope, pk, null,
                                Net.Sf.Pkcs11.EtokenExtensions.Wrapper.pkcs7SignFlags.DETACHED_SIGNATURE);

                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Filter = "Файлы P7C (*.p7c)|*.p7c";
                            saveFileDialog.FilterIndex = 0;
                            saveFileDialog.RestoreDirectory = true;
                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                using (BinaryWriter binWriter = new BinaryWriter(File.Open(saveFileDialog.FileName, FileMode.Create)))
                                {
                                    binWriter.Write(lEnvelope);
                                    MessageBox.Show("Файл успешно подписан");
                                }
                            }

                            // Показываем
                            /*MessageBox.Show(this, "Signed successfully!" + Environment.NewLine +
                                "Initial Data:" + BitConverter.ToString(data) + Environment.NewLine +
                                "Envelope:" + Environment.NewLine + BitConverter.ToString(lEnvelope));
                            */

                            // Проверяем
                            /*aEtokenModule.pkcs7Verify(lEnvelope, data);
                            MessageBox.Show(this, "Signature successfully verified!");
                             */
                        };
                    });
                }
                finally
                {
                    bSign.Enabled = true;
                }
            }
        }
        public void DoWithFirstEtokenSlotWhileLoggedIn(EtokenSessionAction aAction)
        {
            DoWithFirstEtokenSlot(delegate(EtokenModule aEtokenModule, Slot aSlot)
            {
                // Открываем сессию
                Session session = aSlot.Token.OpenSession(false);
                session.Login(UserType.USER, pIN);

                // Производим действия
                try
                {
                    if (aAction != null)
                        aAction(aEtokenModule, session);
                }
                finally
                {
                    // Закрываем сессию
                    session.Logout();
                }
            });
        }
        public void DoWithFirstSlot(SlotAction aAction)
        {
            try
            {
                // Открываем библиотеку
                Module lModule = Module.GetInstance(dLLPath);

                using (lModule)
                {
                    // Находим слот
                    Slot[] lSlots = lModule.GetSlotList(true);
                    if (lSlots.Length == 0)
                    {
                        MessageBox.Show(this, "Отсутствует ключ");
                        return;
                    }

                    if (aAction != null)
                        aAction(lSlots[0]);
                }
            }
            catch (Exception E)
            {
                MessageBox.Show(this, E.ToString());
            }
        }
        public void DoWithFirstEtokenSlot(EtokenSlotAction aAction)
        {
            try
            {
                // Открываем библиотеку
                EtokenModule lModule = EtokenModule.GetInstance(dLLPath);

                using (lModule)
                {
                    // Список слотов
                    Slot[] lSlots = lModule.GetSlotList(true);
                    if (lSlots.Length == 0)
                    {
                        MessageBox.Show(this, "Отсутствует ключ");
                        return;
                    }

                    if (aAction != null)
                        aAction(lModule, lSlots[0]);
                }

            }
            catch (Exception E)
            {
                MessageBox.Show(this, E.ToString());
            }
        }
        public delegate void EtokenSlotAction(EtokenModule aEtokenModule, Slot aSlot);
        public delegate void EtokenSessionAction(EtokenModule aEtokenModule, Session aSession);
        public delegate void SlotAction(Slot aSlot);

    }
}

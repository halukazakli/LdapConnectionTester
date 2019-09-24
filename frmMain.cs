using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.DirectoryServices.Protocols;
using System.Security.Cryptography.X509Certificates;
using System.Net;

namespace LdapConnectionTester
{
    public partial class frmMain : Form
    {
        static string _server = "";
        static AuthType _authType = AuthType.Ntlm;
        static int _protocolVersion = 3;
        static bool _ssl = false;
        static string _certFile = "";
        static string _user = "";
        static string _pass = "";

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            PopulateAuthTypes();
        }

        void PopulateAuthTypes()
        {
            try
            {
                int index = -1;
                cbAuthType.Items.Clear();
                foreach (var item in Enum.GetValues(typeof(AuthType)).Cast<AuthType>())
                {
                    index++;
                    cbAuthType.Items.Add(item);

                    if (item == AuthType.Ntlm)
                        cbAuthType.SelectedIndex = index;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[PopulateAuthTypes] " + ex.Message);
            }
        }

       

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                _server = txtServer.Text;
                _authType = (AuthType)cbAuthType.SelectedItem;
                _protocolVersion = Convert.ToInt32(nudProtokolVersion.Value);
                _ssl = chkSsl.Checked;
                _certFile = txtCertFile.Text;

                Cursor.Current = Cursors.WaitCursor;
                var connected = Connect();
                Cursor.Current = Cursors.Default;

                MessageBox.Show("Connected.");
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(ex.Message);
            }
        }

        static bool Connect()
        {
            using (var ldapConnection = new LdapConnection(_server))
            {
                ldapConnection.AuthType = _authType;
                ldapConnection.SessionOptions.ProtocolVersion = _protocolVersion;
                ldapConnection.SessionOptions.SecureSocketLayer = _ssl;
                
                if (_ssl && !string.IsNullOrEmpty(_certFile))
                    ldapConnection.SessionOptions.VerifyServerCertificate = new VerifyServerCertificateCallback(ServerCallback);
                
                ldapConnection.Credential = new NetworkCredential(_user, _pass);
                ldapConnection.Bind();
                return true;
            }
        }
        
        static bool ServerCallback(LdapConnection connection, X509Certificate certificate)
        {
            try
            {
                X509Certificate expectedCert = X509Certificate.CreateFromCertFile(_certFile);
                if (expectedCert.Equals(certificate))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[Certificate Error] " + ex.Message);
                return false;
            }
        }

        private void btnOpenFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var dialog = openFileDialog1.ShowDialog();
            if (dialog == System.Windows.Forms.DialogResult.OK)
            {
                txtCertFile.Text = openFileDialog1.FileName;
            }
        }
    }
}

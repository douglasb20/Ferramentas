using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SuperSocket.WebSocket;
using Npgsql;

namespace Ferramentas
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static WebSocketServer server = new WebSocketServer();
        public static NpgsqlConnection npg;
        public static SqlConnection sqlserver = new SqlConnection();
        public static int status;
        public static string msgError;
        

        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("User32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            const int WM_NCPAINT = 0x85;
            if (m.Msg == WM_NCPAINT)
            {
                IntPtr hdc = GetWindowDC(m.HWnd);
                if ((int)hdc != 0)
                {
                    Graphics g = Graphics.FromHdc(hdc);
                    g.FillRectangle(Brushes.Green, new Rectangle(0, 0, 4800, 23));
                    g.Flush();
                    ReleaseDC(m.HWnd, hdc);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(50,50,50);
           

        }

        private void label2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            moverForm();
        }

        public void moverForm()
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            moverForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var pergunta = "Deseja fazer correção do banco de dados em modo Suspect?/Emergency?\n\nTenha certeza que as permissões do banco estejam corretas, senão essa ação não funcionará corretamente.";
                if (MessageBox.Show(pergunta, "Ferramentas", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {

                    btSuspect.Enabled = false;
                    sqlserver.Close();
                    setConnVR(txtServer.Text);
                    SqlCommand cmd = sqlserver.CreateCommand();

                    lblResposta.Visible = true;

                    // Alterando para Master
                    lblResposta.Text = "Colocando database em modo de emergência";
                    cmd.CommandText = "USE Master";
                    //cmd.();

                    cmd = sqlserver.CreateCommand();

                    // Coloca o database em modo de emergência
                    lblResposta.Text = "Colocando database em modo de emergência";
                    cmd.CommandText = "ALTER DATABASE [ETrade] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "ALTER DATABASE [ETrade] SET EMERGENCY";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "DBCC CHECKDB('ETrade', REPAIR_ALLOW_DATA_LOSS) WITH NO_INFOMSGS, ALL_ERRORMSGS";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "ALTER DATABASE ETrade SET MULTI_USER";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "EXEC sp_resetstatus 'ETrade'";
                    cmd.ExecuteNonQuery();

                    /*
                    // Altera o database para SINGLE_USER, ou seja, só um usuário pode estar conectado
                    lblResposta.Text = "Alterando database para SINGLE_USER, ou seja, só um usuário pode estar conectado";
                    cmd.CommandText = "ALTER DATABASE ETrade SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                    cmd.ExecuteNonQuery();

                    // Realiza o comando para reparo do databaseRestarta o status do database
                    cmd.CommandText = "DBCC CHECKDB('ETrade', REPAIR_ALLOW_DATA_LOSS) WITH NO_INFOMSGS, ALL_ERRORMSGS";
                    cmd.ExecuteNonQuery();

                    // Volta a base de dados para multiplos usuáriosRestarta o status do database
                    cmd.CommandText = "ALTER DATABASE ETrade SET MULTI_USER";
                    cmd.ExecuteNonQuery();

                    // Restarta o status do database
                    cmd.CommandText = "EXEC sp_resetstatus 'ETrade'";
                    cmd.ExecuteNonQuery();*/

                    btSuspect.Enabled = true;
                    lblResposta.Visible = false;

                    MessageBox.Show("Executado com sucesso.\nVerifique se o banco de dados foi corrigido.", "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            catch(Exception err)
            {
                MessageBox.Show("Erro ao arrumar banco.\nMotivo: " + err.Message, "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btSuspect.Enabled = true;
                lblResposta.Visible = false;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
        public bool setConnVR(string host, string banco = "ETrade")
        {
            msgError = "";

            try
            {
                
                DataTable qry = new DataTable();
                sqlserver = new SqlConnection(@"Data Source=" + host + ";Initial Catalog=; User ID=sa;Password=senha");

                /*SqlCommand cmd = new SqlCommand(@"IF EXISTS(
                  SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                  WHERE TABLE_NAME = @table) 
                  SELECT 1 ELSE SELECT 0", connVR);*/
                sqlserver.Open();
                return true;
                /*cmd.Parameters.Add("@table", SqlDbType.NVarChar).Value = "Produtos";
                int exists = (int)cmd.ExecuteScalar();
                if (exists == 1) tabela = "Produtos";
                else tabela = "Produto";

                SqlDataAdapter da = new SqlDataAdapter("Select * from " + tabela + " where codigo_fabricante1='2'", connVR);
                da.Fill(qry);


                //MessageBox.Show(qry.Rows[0]["Nome"].ToString().Trim(), "Migrador de Banco", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);*/

            }
            catch (Exception err)
            {
                msgError = err.Message;
                return false;
            }
        }
        public void setConnect(string host, int porta, string user, string pass, string banco)
        {
            status = 1;
            msgError = "";
            try
            {
                npg = new NpgsqlConnection("Server=" + host + ";Port=" + porta + ";User Id=" + user + ";Password=" + pass + ";Database=" + banco + ";");
                npg.Open();

            }
            catch (Exception err)
            {
                msgError = err.Message;
                status = 0;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var banco = "";

            if (rdLeCheff.Checked) banco = "SOFTMOBILE";
            if (rdLeStore.Checked) banco = "RP";

            try
            {
                setConnect(txtHost.Text, int.Parse( txtPorta.Text ), txtUser.Text, txtPass.Text, banco);
                DataTable id = new DataTable();

                NpgsqlDataAdapter da = new NpgsqlDataAdapter("select id_fornecedor from fornecedor order by id_fornecedor desc limit 1 ", npg);
                da.Fill(id);

                int id_fn = int.Parse(id.Rows[0]["id_fornecedor"].ToString());

                NpgsqlCommand cmd = npg.CreateCommand();
                cmd.CommandText = "ALTER SEQUENCE fornecedor_id_fornecedor_seq RESTART WITH " + (id_fn + 1);
                cmd.ExecuteNonQuery();

                if (status == 1) MessageBox.Show("Próximo ID de fornecedor será " + (id_fn +1), "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Asterisk );
            }
            catch(Exception err)
            {
                MessageBox.Show("Erro no comando.\nMotivo: " + err.Message,"Ferramentas",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            

        }

        private void btConect_Click(object sender, EventArgs e)
        {
            if (setConnVR(txtServer.Text))
            {

                pnlRP.Enabled = false;
                txtServer.Enabled = false;

                btSuspect.Enabled = true;
                btCest.Enabled = true;
                button3.Enabled = true;
                btConect.Enabled = false;
            }
            else
            {
                MessageBox.Show(msgError, "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button3.Enabled = false;
                btConect.Enabled = true;
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (sqlserver.State != ConnectionState.Closed)
            {

                pnlRP.Enabled = true;
                txtServer.Enabled = true;

                btCest.Enabled = false;
                sqlserver.Close();

                button3.Enabled = false;
                btConect.Enabled = true;

            }

        }

        private void btCest_Click(object sender, EventArgs e)
        {
            try
            {
                SqlCommand cmd = sqlserver.CreateCommand();
                DataTable tabela = new DataTable();
                string comando = "";

                Object aTabela = "";

                cmd.CommandText = "Use ETrade";
                cmd.ExecuteNonQuery();

                comando += "select schema_name(t.schema_id) as schema_name, t.name as table_name, t.create_date, t.modify_date ";
                comando += "from sys.tables t ";
                comando += "where t.name = 'Produto' ";
                comando += "order by schema_name, table_name ";

                SqlDataAdapter da = new SqlDataAdapter(comando, sqlserver);
                da.Fill(tabela);
                
                if(tabela.Rows.Count == 0)
                {
                    comando += "select schema_name(t.schema_id) as schema_name, t.name as table_name, t.create_date, t.modify_date ";
                    comando += "from sys.tables t ";
                    comando += "where t.name = 'Produtos' ";
                    comando += "order by schema_name, table_name ";

                    da = new SqlDataAdapter(comando, sqlserver);
                    da.Fill(tabela);

                    aTabela = tabela.Rows[0]["table_name"];
                }else
                {
                    aTabela = tabela.Rows[0]["table_name"];
                }


                cmd.CommandText = "update " + aTabela + " set CEST= '00000000-0000-0000-0000-000000000000'";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "delete from cest where codigo='' or ncm='' ";
                cmd.ExecuteNonQuery();

                MessageBox.Show("Executado com sucesso.", "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch(Exception err)
            {
                MessageBox.Show("Não foi possível executar a ação.\nMotivo: " + err.Message, "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
    }
}

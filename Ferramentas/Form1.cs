using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Npgsql;
using System.ServiceProcess;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Ferramentas
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static SqlConnection sqlserver = new SqlConnection();
        public static NpgsqlConnection npg;
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
            lblVersao.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            

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

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            moverForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                checkServiceFirst("MSSQL$" + nomeServico(txtServer.Text));
                var pergunta = "Deseja fazer correção do banco de dados em modo Suspect?/Emergency?\n\nTenha certeza que as permissões do banco estejam corretas, senão essa ação não funcionará corretamente.";
                if (MessageBox.Show(pergunta, "Ferramentas", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {

                    btSuspect.Enabled = false;
                    sqlserver.Close();
                    setConnVR(txtServer.Text);
                    SqlCommand cmd = sqlserver.CreateCommand();


                    // Alterando para Master
                    cmd.CommandText = "USE Master";
                    //cmd.();

                    cmd = sqlserver.CreateCommand();


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

                    btSuspect.Enabled = true;

                    MessageBox.Show("Executado com sucesso.\nVerifique se o banco de dados foi corrigido.", "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            catch(Exception err)
            {
                erroMsg(err);
                btSuspect.Enabled = true;
            }
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            var banco = "";

            if (rdLeCheff.Checked) banco = "SOFTMOBILE";
            if (rdLeStore.Checked) banco = "RP";

            try
            {

                checkServiceFirst("postgresql");
                setConnect(txtHost.Text, int.Parse( txtPorta.Text ), txtUser.Text, txtPass.Text, banco);
                DataTable id = new DataTable();

                NpgsqlDataAdapter da = new NpgsqlDataAdapter("select id_fornecedor from fornecedor order by id_fornecedor desc limit 1 ", npg);
                da.Fill(id);

                if (id.Rows.Count == 0) throw new Exception("Não há nenhum fornecedor cadastrado.");

                int id_fn = int.Parse(id.Rows[0]["id_fornecedor"].ToString());

                NpgsqlCommand cmd = npg.CreateCommand();
                cmd.CommandText = "ALTER SEQUENCE fornecedor_id_fornecedor_seq RESTART WITH " + (id_fn + 1);
                cmd.ExecuteNonQuery();

                if (status == 1) MessageBox.Show("Próximo ID de fornecedor será " + (id_fn +1), "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Asterisk );
            }
            catch(Exception err)
            {
                erroMsg(err);
            }
            

        }

        private void btConect_Click(object sender, EventArgs e)
        {
            try
            {
                checkServiceFirst("MSSQL$" + nomeServico(txtServer.Text));
                if (setConnVR(txtServer.Text))
                {

                    pnlRP.Enabled = false;
                    txtServer.Enabled = false;

                    btSuspect.Enabled = true;
                    btCest.Enabled = true;
                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    button5.Enabled = true;
                    btConect.Enabled = false;
                }
                else
                {
                    MessageBox.Show(msgError, "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    button3.Enabled = false;
                    btConect.Enabled = true;
                }

            }catch(Exception err)
            {
                erroMsg(err);
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (sqlserver.State != ConnectionState.Closed)
            {

                try
                {
                    sqlserver.Close();
                    checkServiceFirst("postgresql");

                    pnlRP.Enabled = true;
                }
                catch(Exception err)
                {
                    
                }
                finally
                {
                    txtServer.Enabled = true;

                    btCest.Enabled = false;
                    button1.Enabled = false;
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button4.Enabled = false;
                    button5.Enabled = false;
                    btConect.Enabled = true;

                }

            }

        }
        
        private void btCest_Click(object sender, EventArgs e)
        {
            try
            {
                SqlCommand cmd = sqlserver.CreateCommand();
                DataTable tabela = new DataTable();
                string comando = "";

                string aTabela = "";

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

                    aTabela = tabela.Rows[0]["table_name"].ToString();
                }else
                {
                    aTabela = tabela.Rows[0]["table_name"].ToString();
                }


                cmd.CommandText = "update " + aTabela + " set CEST= '00000000-0000-0000-0000-000000000000'";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "delete from cest where codigo='' or ncm='' ";
                cmd.ExecuteNonQuery();

                msgSucesso();
            }
            catch(Exception err)
            {
                erroMsg(err);
            }
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            frmAcao f2 = new frmAcao(1);
            f2.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmAcao f2 = new frmAcao(2);
            f2.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            frmAcao f2 = new frmAcao(3);
            f2.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {

                frmAcao f2 = new frmAcao(4);
                f2.ShowDialog();
            }catch(Exception err)
            {
                erroMsg(err);
            }
        }

        // Somente Funções
        public void moverForm()
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
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

        public void zeraReg()
        {
            try
            {

                // ExportImagesFromDatabase();
                SqlCommand cmd = sqlserver.CreateCommand();
                cmd.CommandText = "Use ETrade";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"update Configuracao set Valor='' where Len(Valor) > 100 ";
                cmd.ExecuteNonQuery();

                //if( MessageBox.Show("Abra o sistema e quando tiver totalmente aberto, aperte OK para adicionar a imagem na filial.\n\nClicando em Cancelar finaliza a operação sem adicionar a imagem, porem fica um backup da imagem na pasta C:\\temp\\temp.jpg. \n\nSistema abriu totalmente? ", "Ferramentas", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                //{
                //    cmd.CommandText = @"UPDATE Filial_Foto SET Foto = (SELECT MyImage.* from Openrowset(Bulk 'C:\temp\temp.jpg', Single_Blob) MyImage) where Filial = 1";
                //    cmd.ExecuteScalar();

                //}

                msgSucesso();
            }
            catch (Exception err)
            {
                erroMsg(err);
            }
        }

        public void retiraVinculo(string sequencia)
        {
            try
            {
                SqlCommand cmd = sqlserver.CreateCommand();
                cmd.CommandText = "USE ETrade";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "UPDATE MOVIMENTO SET NFCE_IDENTIFICADOR='00000000-0000-0000-0000-000000000000' WHERE sequencia='" + sequencia + "'";
                cmd.ExecuteNonQuery();

                msgSucesso();

            }
            catch (Exception err)
            {
                erroMsg(err);
            }
        }

        public void checkServiceFirst(string svr)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController sv in services)
            {
                if (sv.ServiceName.Length >= 10)
                {
                    if (sv.ServiceName.Contains(svr))
                    {

                        if (sv.Status.ToString() != "Running") throw new Exception("Serviço " + sv.DisplayName + " não está em execução.");
                    }
                }
            }
        }

        public bool setConnVR(string host, string banco = "ETrade")
        {
            msgError = "";

            try
            {

                DataTable qry = new DataTable();
                sqlserver = new SqlConnection(@"Data Source=" + host + ";Initial Catalog=; User ID=sa;Password=senha");
                sqlserver.Open();
                return true;

            }
            catch (Exception err)
            {
                msgError = err.Message;
                return false;
            }
        }

        public void mudaNfce(string sequencia, int tipo)
        {
            try
            {
                SqlCommand cmd = sqlserver.CreateCommand();
                cmd.CommandText = "USE ETrade";
                cmd.ExecuteNonQuery();

                if (tipo == 0)
                {
                    cmd.CommandText = "update movimento_nfe set inutilizada=1 where movimento__ide IN( SELECT IDE FROM Movimento WHERE Sequencia='" + sequencia + "')";
                    cmd.ExecuteNonQuery();

                }
                else
                {
                    cmd.CommandText = "update movimento_nfe set codigo_status=100 where movimento__ide IN( SELECT IDE FROM Movimento WHERE Sequencia='" + sequencia + "')";
                    cmd.ExecuteNonQuery();
                }


                msgSucesso();

            }
            catch (Exception err)
            {
                erroMsg(err);
            }
        }

        public string nomeServico(string texto)
        {
            string[] separado;

            if (texto.Contains(","))
            {
                separado = texto.Split(',');
                texto = separado[0];
            }

            separado = texto.Split('\\');
            texto = separado[1];

            return texto;
        }

        public void erroDecimais(string sequencia)
        {

            try
            {
                SqlCommand cmd = sqlserver.CreateCommand();
                cmd.CommandText = "USE ETrade";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "update Movimento_Produto set Valor_Total = Qtde* Valor_Unit where Movimento__Ide in( SELECT IDE FROM Movimento WHERE Sequencia='" + sequencia + "')";
                cmd.ExecuteNonQuery();

                msgSucesso();

            }
            catch (Exception err)
            {
                erroMsg(err);
            }
        }

        public void msgSucesso()
        {
            MessageBox.Show("Executado com sucesso.", "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        public void erroMsg(Exception mesage, string tipo = null)
        {
            if (tipo == "1")
            {

                MessageBox.Show(mesage.Message, "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {

                MessageBox.Show("Não foi possível executar a ação.\nMotivo: " + mesage.Message, "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public DataTable GetImagesFromDatabase(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new NullReferenceException("Parameter query can't be null or empty");
            }

            SqlCommand cmd = sqlserver.CreateCommand();

            cmd.CommandText = "use ETrade";
            cmd.ExecuteScalar();

            cmd.CommandText = query;

            DataTable result = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(result);

            return result;
        }

        public void ExportImagesFromDatabase()
        {
            var exportPath = @"C:\Temp";
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
            var imagesTable = GetImagesFromDatabase("select * from Filial_Foto");
            foreach (DataRow row in imagesTable.Rows)
            {
                if (row.ItemArray.Length > 0)
                {
                    if (row[0] != null)
                    {
                        var imageBytes = (byte[])row["Foto"];
                        if (imageBytes.Length > 0)
                        {
                            using (var convertedImage = new Bitmap(new MemoryStream(imageBytes)))
                            {
                                var fileName = Path.Combine(exportPath, "temp.jpg");
                                if (File.Exists(fileName))
                                {
                                    File.Delete(fileName);
                                }
                                convertedImage.Save(fileName);
                            }
                        }
                    }
                }
            }
        }

       
    }
}

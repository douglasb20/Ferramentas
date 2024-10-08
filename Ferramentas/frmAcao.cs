﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ferramentas
{
    public partial class frmAcao : Form
    {
        public static int tipAcao;

        public frmAcao(int acao)
        {
            InitializeComponent();
            tipAcao = acao;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 objj;

            objj = (Form1)Application.OpenForms["Form1"];

            if (txtSeq.Text != "")
            {

                switch (tipAcao)
                {
                    case 1:
                        objj.erroDecimais(txtSeq.Text);
                    break;

                    case 2:
                        objj.mudaNfce(txtSeq.Text, cboTipo.SelectedIndex);
                    break;

                    case 3:
                        objj.retiraVinculo(txtSeq.Text);
                    break;
                    case 4:
                        if (txtSeq.Text == "237701")
                        {
                            objj.zeraReg();
                            
                        }else
                        {
                            throw new Exception("Erro na senha");
                        }
                    break;

                    default:
                    break;
                }
                this.Close();
            }else
            {
                MessageBox.Show("Campo sequencia não pode ser vazia.", "Ferramentas", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btCancel_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmAcao_Load(object sender, EventArgs e)
        {
            this.Size = new Size(this.Size.Width, 110);

            switch (tipAcao)
            {
                case 2:
            
                    cboTipo.Visible = true;
                    cboTipo.SelectedIndex = 0;

                    this.Size = new Size(this.Size.Width, 170);
                    panel1.Location = new Point(panel1.Location.X, 55);
                break;

                case 4:

                    lblSeq.Text = "Senha";
                    txtSeq.PasswordChar = '*';
                break;

                case 99:
            
                    txtChave.Visible = true;
                    lblMuda.Text = "Chave de acesso";
                    this.Size = new Size(this.Size.Width, 170);
                    panel1.Location = new Point(panel1.Location.X, 55);
                break;

            }

            txtSeq.Select();
        }

        private void txtSeq_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == (char)13)
            {
                btOk.PerformClick();
            }
        }
    }
}

using System;
using System.Data;
using System.Drawing; // Görsel konumlandırma ve renklendirme için gerekli
using System.Windows.Forms;
using System.Data.SqlClient; // Veri tabanı bağlantısı için şart olan kütüphane

namespace ışıl_proje
{
    public partial class Form1 : Form
    {
        // SQL Server yerel bağlantı adresiniz [Döküman Bölüm 1.3]
        // Adres "localhost" olarak eşitlenmiştir.
        private string connectionString = "Data Source=localhost;Initial Catalog=DAYS_DB;Integrated Security=True";

        // Form elemanlarını kod ile tanımlıyoruz (Tasarım ekranı boş olsa bile çalışırlar)
        private Label lblKullaniciAdi;
        private Label lblSifre;
        private TextBox txtKullaniciAdi;
        private TextBox txtSifre;
        private Button btnGiris;

        public Form1()
        {
            InitializeComponent();
            DinamikTasarimOlustur(); // Form açılırken elemanları ekrana çizecek yardımcı fonksiyon
        }

        // Tasarım ekranı boş olduğu için elemanları formun üzerine kodla yerleştiren fonksiyon
        private void DinamikTasarimOlustur()
        {
            // Form Temel Ayarları [Döküman Bölüm 5.1]
            this.Text = "DAYS - Dijital Askı Yönetim Sistemi Giriş";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240); // Açık gri arka plan

            // Kullanıcı Adı Etiketi (Label)
            lblKullaniciAdi = new Label();
            lblKullaniciAdi.Text = "Kullanıcı Adı:";
            lblKullaniciAdi.Location = new Point(50, 50);
            lblKullaniciAdi.Size = new Size(100, 20);
            lblKullaniciAdi.Font = new Font("Arial", 10, FontStyle.Bold);

            // Kullanıcı Adı Metin Kutusu (TextBox)
            txtKullaniciAdi = new TextBox();
            txtKullaniciAdi.Location = new Point(160, 48);
            txtKullaniciAdi.Size = new Size(180, 22);

            // Şifre Etiketi (Label)
            lblSifre = new Label();
            lblSifre.Text = "Şifre:";
            lblSifre.Location = new Point(50, 100);
            lblSifre.Size = new Size(100, 20);
            lblSifre.Font = new Font("Arial", 10, FontStyle.Bold);

            // Şifre Metin Kutusu (TextBox)
            txtSifre = new TextBox();
            txtSifre.Location = new Point(160, 98);
            txtSifre.Size = new Size(180, 22);
            txtSifre.PasswordChar = '*'; // Şifre girilirken karakterleri gizler

            // Giriş Yap Butonu (Button)
            btnGiris = new Button();
            btnGiris.Text = "Giriş Yap";
            btnGiris.Location = new Point(160, 150);
            btnGiris.Size = new Size(180, 35);
            btnGiris.BackColor = Color.SteelBlue; // Şık kurumsal mavi
            btnGiris.ForeColor = Color.White;
            btnGiris.Font = new Font("Arial", 10, FontStyle.Bold);
            btnGiris.FlatStyle = FlatStyle.Flat;

            // Butona tıklanınca çalışacak fonksiyonu (Event) bağlıyoruz
            btnGiris.Click += new EventHandler(btnGiris_Click);

            // Oluşturduğumuz tüm elemanları formun görsel havuzuna ekliyoruz
            this.Controls.Add(lblKullaniciAdi);
            this.Controls.Add(txtKullaniciAdi);
            this.Controls.Add(lblSifre);
            this.Controls.Add(txtSifre);
            this.Controls.Add(btnGiris);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Olası event çakışmalarını önlemek için boş bırakılmıştır.
        }

        // Giriş Yap butonuna tıklandığında çalışan ana kod bloğu [Döküman Bölüm 5.1]
        private void btnGiris_Click(object sender, EventArgs e)
        {
            string kullaniciAdi = txtKullaniciAdi.Text;
            string sifre = txtSifre.Text;

            // Alan kontrolü
            if (string.IsNullOrEmpty(kullaniciAdi) || string.IsNullOrEmpty(sifre))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection baglanti = new SqlConnection(connectionString))
            {
                // SQL Injection riskine karşı PARAMETRELİ sorgu yapısı [Döküman Bölüm 3.2]
                string sorgu = "SELECT Rol FROM Kullanicilar WHERE kullanici_adi = @kullanici AND sifre_hash = @sifre AND aktif_mi = 1";
                SqlCommand komut = new SqlCommand(sorgu, baglanti);
                komut.Parameters.AddWithValue("@kullanici", kullaniciAdi.Trim());
                komut.Parameters.AddWithValue("@sifre", sifre.Trim());

                try
                {
                    baglanti.Open();
                    object sonuc = komut.ExecuteScalar(); // Sorgudan dönen Rol bilgisini okur [Döküman Bölüm 4.1]

                    if (sonuc != null) // Kullanıcı bulunduysa
                    {
                        string gelenRol = sonuc.ToString();
                        MessageBox.Show("Sisteme başarıyla giriş yapıldı!\nRolünüz: " + gelenRol, "DAYS Giriş Onayı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // [Döküman Bölüm 5.2] Dashboard (Ana Panel) formunu açıyoruz ve rolü gönderiyoruz
                        FrmDashboard dashboard = new FrmDashboard(gelenRol);
                        dashboard.Show();

                        this.Hide(); // Giriş ekranını gizliyoruz
                    }
                    else
                    {
                        MessageBox.Show("Kullanıcı adı veya şifre hatalı ya da hesabınız aktif değil!", "Giriş Başarısız", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veri tabanı bağlantı hatası:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
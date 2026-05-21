using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace ışıl_proje
{
    public partial class FrmDashboard : Form
    {
        private string connectionString = "Data Source=localhost;Initial Catalog=DAYS_DB;Integrated Security=True";
        private string aktifRol;

        // Mevcut elemanlar
        private Panel pnlHeader;
        private Label lblBaslik;
        private DataGridView dgvUrunler;
        private Button btnIslemYap;
        private Label lblOzetBilgi;
        private Button btnCikis;

        // YÖNETİCİYE ÖZEL YENİ ELEMANLAR
        private Label lblKullanicilarBaslik;
        private DataGridView dgvKullanicilar;

        public FrmDashboard(string rol)
        {
            InitializeComponent();
            this.aktifRol = rol;
            DashboardTasariminiCiz();
            VerileriYukle();
        }

        private void DashboardTasariminiCiz()
        {
            this.Text = "DAYS - Dijital Askı Yönetim Sistemi Ana Panel";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 245);

            // Eğer giren yöneticiyse ekranı yanlamasına genişletiyoruz (İki tablo sığsın diye)
            if (aktifRol == "yonetici")
                this.Size = new Size(1150, 520);
            else
                this.Size = new Size(800, 520);

            // Üst Panel (Header)
            pnlHeader = new Panel();
            pnlHeader.Size = new Size(this.Width, 60);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.BackColor = Color.SteelBlue;

            lblBaslik = new Label();
            lblBaslik.Text = "DİJİTAL ASKI SİSTEMİ - " + aktifRol.ToUpper() + " PANELİ";
            lblBaslik.Location = new Point(20, 18);
            lblBaslik.Size = new Size(600, 30);
            lblBaslik.ForeColor = Color.White;
            lblBaslik.Font = new Font("Arial", 14, FontStyle.Bold);
            pnlHeader.Controls.Add(lblBaslik);
            this.Controls.Add(pnlHeader);

            lblOzetBilgi = new Label();
            lblOzetBilgi.Text = aktifRol == "yonetici" ? "Sistemdeki Aktif Askı Ürünleri (Sol) ve Kayıtlı Kullanıcı Hesapları (Sağ)" : "Kritik Stok Uyarıları ve Aktif Bağış Bilgileri Burada Listelenecek.";
            lblOzetBilgi.Location = new Point(20, 80);
            lblOzetBilgi.Size = new Size(500, 30);
            lblOzetBilgi.Font = new Font("Arial", 10, FontStyle.Italic);
            this.Controls.Add(lblOzetBilgi);

            // 1. Tablo: Ürünler Tablosu (Genişliği yöneticiye göre esniyor)
            dgvUrunler = new DataGridView();
            dgvUrunler.Location = new Point(20, 120);
            dgvUrunler.Size = aktifRol == "yonetici" ? new Size(550, 280) : new Size(740, 280);
            dgvUrunler.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUrunler.ReadOnly = true;
            this.Controls.Add(dgvUrunler);

            // --- YÖNETİCİYE ÖZEL KULLANICI TABLOSUNU ÇİZİYORUZ ---
            if (aktifRol == "yonetici")
            {
                lblKullanicilarBaslik = new Label();
                lblKullanicilarBaslik.Text = "👥 Kayıtlı Bağışçı ve Alıcı Listesi";
                lblKullanicilarBaslik.Location = new Point(600, 80);
                lblKullanicilarBaslik.Size = new Size(300, 30);
                lblKullanicilarBaslik.Font = new Font("Arial", 11, FontStyle.Bold);
                lblKullanicilarBaslik.ForeColor = Color.DarkSlateGray;
                this.Controls.Add(lblKullanicilarBaslik);

                dgvKullanicilar = new DataGridView();
                dgvKullanicilar.Location = new Point(600, 120);
                dgvKullanicilar.Size = new Size(500, 280);
                dgvKullanicilar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvKullanicilar.ReadOnly = true;
                this.Controls.Add(dgvKullanicilar);
            }

            // Dinamik İşlem Butonu
            btnIslemYap = new Button();
            btnIslemYap.Size = new Size(180, 40);
            btnIslemYap.Location = aktifRol == "yonetici" ? new Point(920, 420) : new Point(580, 420);
            btnIslemYap.Font = new Font("Arial", 10, FontStyle.Bold);
            btnIslemYap.FlatStyle = FlatStyle.Flat;

            if (aktifRol == "yonetici")
            {
                btnIslemYap.Text = "Sistem ve Ürün Onayları";
                btnIslemYap.BackColor = Color.DarkOrange;
                btnIslemYap.ForeColor = Color.White;
            }
            else if (aktifRol == "bagisci")
            {
                btnIslemYap.Text = "Askıya Yeni Ürün Ekle";
                btnIslemYap.BackColor = Color.ForestGreen;
                btnIslemYap.ForeColor = Color.White;
            }
            else if (aktifRol == "alici")
            {
                btnIslemYap.Text = "Seçili Ürünü Rezerve Et";
                btnIslemYap.BackColor = Color.Crimson;
                btnIslemYap.ForeColor = Color.White;
            }
            btnIslemYap.Click += new EventHandler(btnIslemYap_Click);
            this.Controls.Add(btnIslemYap);

            // Güvenli Çıkış Butonu
            btnCikis = new Button();
            btnCikis.Text = "🚪 Güvenli Çıkış";
            btnCikis.Location = new Point(20, 420);
            btnCikis.Size = new Size(150, 40);
            btnCikis.Font = new Font("Arial", 10, FontStyle.Bold);
            btnCikis.FlatStyle = FlatStyle.Flat;
            btnCikis.BackColor = Color.FromArgb(220, 53, 69);
            btnCikis.ForeColor = Color.White;
            btnCikis.Click += new EventHandler(btnCikis_Click);
            this.Controls.Add(btnCikis);
        }

        private void VerileriYukle()
        {
            using (SqlConnection baglanti = new SqlConnection(connectionString))
            {
                // 1. Ürünleri Yükle
                string urunSorgu = "SELECT urun_ad AS [Ürün Adı], stok_adedi AS [Stok Miktarı], birim_fiyat AS [Değer (TL)] FROM Urunler WHERE stok_adedi > 0";
                SqlDataAdapter daUrun = new SqlDataAdapter(urunSorgu, baglanti);
                DataTable dtUrun = new DataTable();

                try
                {
                    baglanti.Open();
                    daUrun.Fill(dtUrun);
                    dgvUrunler.DataSource = dtUrun;

                    // 2. Eğer Yönetici ise Kullanıcı Listesini de SQL'den çekip sağdaki tabloya bas
                    if (aktifRol == "yonetici" && dgvKullanicilar != null)
                    {
                        string kullanıcıSorgu = "SELECT kullanici_adi AS [Kullanıcı Adı], Rol AS [Sistem Rolü], aktif_mi AS [Hesap Durumu] FROM Kullanicilar";
                        SqlDataAdapter daUser = new SqlDataAdapter(kullanıcıSorgu, baglanti);
                        DataTable dtUser = new DataTable();
                        daUser.Fill(dtUser);
                        dgvKullanicilar.DataSource = dtUser;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message);
                }
            }
        }

        private void btnIslemYap_Click(object sender, EventArgs e)
        {
            if (aktifRol == "yonetici")
            {
                MessageBox.Show("Yönetici Paneli: Askıdaki bekleyen tüm ürünler ve kullanıcı logları incelenerek onaylandı!",
                                "Sistem Onay Merkezi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                VerileriYukle();
            }
            else if (aktifRol == "bagisci")
            {
                Form secimFormu = new Form();
                secimFormu.Text = "DAYS - Bağış Ürünü Seçim Paneli";
                secimFormu.Size = new Size(350, 280);
                secimFormu.StartPosition = FormStartPosition.CenterScreen;
                secimFormu.FormBorderStyle = FormBorderStyle.FixedDialog;
                secimFormu.MaximizeBox = false;

                string urunAdi = ""; int kategoriId = 3; decimal fiyat = 0; bool secimYapildi = false;

                Button btnMont = new Button { Text = "👕 Kışlık Mont Ekle (500 TL)", Location = new Point(30, 70), Size = new Size(270, 40), BackColor = Color.White, FlatStyle = FlatStyle.Flat };
                btnMont.Click += (s, args) => { urunAdi = "Kışlık Mont"; kategoriId = 3; fiyat = 500; secimYapildi = true; secimFormu.Close(); };

                Button btnErzak = new Button { Text = "📦 Temel Erzak Kolisi Ekle (400 TL)", Location = new Point(30, 120), Size = new Size(270, 40), BackColor = Color.White, FlatStyle = FlatStyle.Flat };
                btnErzak.Click += (s, args) => { urunAdi = "Temel Erzak Kolisi"; kategoriId = 1; fiyat = 400; secimYapildi = true; secimFormu.Close(); };

                Button btnBurs = new Button { Text = "🎓 Eğitim Burs Desteği Ekle (1000 TL)", Location = new Point(30, 170), Size = new Size(270, 40), BackColor = Color.White, FlatStyle = FlatStyle.Flat };
                btnBurs.Click += (s, args) => { urunAdi = "Eğitim Burs Desteği"; kategoriId = 2; fiyat = 1000; secimYapildi = true; secimFormu.Close(); };

                secimFormu.Controls.Add(btnMont); secimFormu.Controls.Add(btnErzak); secimFormu.Controls.Add(btnBurs);
                secimFormu.ShowDialog();

                if (secimYapildi)
                {
                    using (SqlConnection baglanti = new SqlConnection(connectionString))
                    {
                        string sorgu = "INSERT INTO Urunler (urun_ad, kategori_id, stok_adedi, birim_fiyat) VALUES (@urun, @kategori, 1, @fiyat)";
                        SqlCommand komut = new SqlCommand(sorgu, baglanti);
                        komut.Parameters.AddWithValue("@urun", urunAdi);
                        komut.Parameters.AddWithValue("@kategori", kategoriId);
                        komut.Parameters.AddWithValue("@fiyat", fiyat);
                        try { baglanti.Open(); komut.ExecuteNonQuery(); MessageBox.Show($"'{urunAdi}' askıya eklendi!", "Başarılı"); VerileriYukle(); }
                        catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
                    }
                }
            }
            else if (aktifRol == "alici")
            {
                if (dgvUrunler.CurrentRow != null)
                {
                    string secilenUrun = dgvUrunler.CurrentRow.Cells["Ürün Adı"].Value.ToString();
                    using (SqlConnection baglanti = new SqlConnection(connectionString))
                    {
                        string sorgu = "UPDATE Urunler SET stok_adedi = stok_adedi - 1 WHERE urun_ad = @urunAd AND stok_adedi > 0";
                        SqlCommand komut = new SqlCommand(sorgu, baglanti);
                        komut.Parameters.AddWithValue("@urunAd", secilenUrun);
                        try
                        {
                            baglanti.Open();
                            int etkilenen = komut.ExecuteNonQuery();
                            if (etkilenen > 0) { MessageBox.Show($"'{secilenUrun}' rezerve edildi."); VerileriYukle(); }
                        }
                        catch (Exception ex) { MessageBox.Show("Hata: " + ex.Message); }
                    }
                }
            }
        }

        private void btnCikis_Click(object sender, EventArgs e)
        {
            DialogResult cevap = MessageBox.Show("Oturumu kapatmak istediğinize emin misiniz?", "DAYS - Güvenli Çıkış", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (cevap == DialogResult.Yes)
            {
                Form1 girisEkrani = new Form1();
                girisEkrani.Show();
                this.Close();
            }
        }

        private void FrmDashboard_Load(object sender, EventArgs e) { }
    }
}
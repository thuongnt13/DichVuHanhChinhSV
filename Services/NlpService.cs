using CongDichVuSinhVien.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CongDichVuSinhVien.Services
{
    public class NlpService : INlpService
    {
        private readonly CongDichVuSinhVienContext _context;

        // 1. TỪ DỪNG (STOPWORDS): Bổ sung cả từ CÓ DẤU và KHÔNG DẤU để lọc sạch nhiễu
        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Đại từ xưng hô
            "em", "toi", "tôi", "minh", "mình", "chau", "cháu", "thay", "thầy", "co", "cô", "anh", "chi", "chị",
            // Từ nối, từ hỏi, quan hệ từ
            "xin", "cho", "hoi", "hỏi", "lam", "làm", "sao", "de", "để", "o", "ở", "dau", "đâu", "nao", "nào", "gi", "gì",
            "the", "thế", "duoc", "được", "khong", "không", "nhi", "nhỉ", "a", "ạ", "da", "dạ", "nhe", "nhé", "nha", "ha", "hả",
            "giup", "giúp", "voi", "với", "cua", "của", "cac", "các", "nhung", "những", "thi", "thì", "la", "là", "ma", "mà", "va", "và", "hoac", "hoặc",
            "can", "cần", "muon", "muốn", "phai", "phải", "chua", "chưa", "roi", "rồi", "rat", "rất", "qua", "quá", "nen", "nên",
            "bi", "bị", "nay", "này", "do", "đó", "kia", "gap", "gấp", "chut", "chút"
        };

        // 2. TỪ ĐỒNG NGHĨA & TỪ VIẾT TẮT: Ánh xạ GDQP, GDTC, quân sự, thể dục về đúng dịch vụ
        private static readonly Dictionary<string, string> TuDongNghia = new(StringComparer.OrdinalIgnoreCase)
        {
            // --- Giáo dục quốc phòng (GDQP) ---
            { "gdqp", "giáo dục quốc phòng an ninh quân sự gdqp" },
            { "gdqp an", "giáo dục quốc phòng an ninh quân sự gdqp" },
            { "gdqpan", "giáo dục quốc phòng an ninh quân sự gdqp" },
            { "quân sự", "giáo dục quốc phòng an ninh quân sự" },
            { "quan su", "giáo dục quốc phòng an ninh quân sự" },
            { "học quân sự", "giáo dục quốc phòng an ninh quân sự" },
            { "kỳ quân sự", "giáo dục quốc phòng an ninh quân sự" },

            // --- Giáo dục thể chất (GDTC) ---
            { "gdtc", "giáo dục thể chất thể dục gdtc" },
            { "thể dục", "giáo dục thể chất thể dục gdtc" },
            { "the duc", "giáo dục thể chất thể dục gdtc" },
            { "thể chất", "giáo dục thể chất thể dục gdtc" },
            { "the chat", "giáo dục thể chất thể dục gdtc" },
            { "môn thể dục", "giáo dục thể chất thể dục gdtc" },

            // --- Bảng điểm & Học tập ---
            { "gpa", "bảng điểm kết quả học tập tích lũy" },
            { "drl", "điểm rèn luyện" },
            { "bản điểm", "bảng điểm" },
            { "ban diem", "bảng điểm" },
            { "in điểm", "bảng điểm" },
            { "in diem", "bảng điểm" },
            { "trích lục", "bảng điểm kết quả" },
            { "trich luc", "bảng điểm kết quả" },
            { "học bổng", "học bổng bảng điểm kết quả" },
            { "hoc bong", "học bổng bảng điểm kết quả" },
            { "du học", "tiếng anh song ngữ bảng điểm" },
            { "du hoc", "tiếng anh song ngữ bảng điểm" },

            // --- Vay vốn & Chế độ chính sách ---
            { "vay vốn", "giấy xác nhận sinh viên ngân hàng chính sách vay vốn" },
            { "vay von", "giấy xác nhận sinh viên ngân hàng chính sách vay vốn" },
            { "nvqs", "tạm hoãn nghĩa vụ quân sự" },
            { "tạm hoãn", "tạm hoãn nghĩa vụ quân sự" },
            { "tam hoan", "tạm hoãn nghĩa vụ quân sự" },
            { "miễn giảm", "miễn giảm học phí trợ cấp xã hội" },

            // --- Thủ tục khác ---
            { "thực tập", "giấy giới thiệu thực tập doanh nghiệp công ty" },
            { "thuc tap", "giấy giới thiệu thực tập doanh nghiệp công ty" },
            { "mất thẻ", "cấp lại thẻ sinh viên" },
            { "mat the", "cấp lại thẻ sinh viên" },
            { "làm lại thẻ", "cấp lại thẻ sinh viên" },
            { "phúc khảo", "phúc khảo chấm lại bài thi điểm thi" },
            { "phuc khao", "phúc khảo chấm lại bài thi điểm thi" },
            { "tốt nghiệp tạm thời", "giấy chứng nhận tốt nghiệp tạm thời" }
        };

        public NlpService(CongDichVuSinhVienContext context)
        {
            _context = context;
        }

        public async Task<List<NlpSearchResult>> PhanTichNhuCauSinhVienAsync(string cauHoiNguoiDung)
        {
            if (string.IsNullOrWhiteSpace(cauHoiNguoiDung))
                return new List<NlpSearchResult>();

            var trainData = await _context.DuLieuHuanLuyenNlp.ToListAsync();
            var dichVuList = await _context.DichVu.Where(d => d.KichHoat == true).ToListAsync();

            if (!dichVuList.Any()) return new List<NlpSearchResult>();

            var corpus = new List<(string KyHieu, string Text, string RawSample)>();

            // Nạp dữ liệu từ bảng huấn luyện câu hỏi mẫu
            foreach (var item in trainData)
            {
                corpus.Add((item.KyHieuDichVu, item.CauHoiMau, item.CauHoiMau));
            }

            // Nạp dữ liệu từ danh mục dịch vụ chính thức
            foreach (var dv in dichVuList)
            {
                string textMoTa = $"{dv.TenDichVu} {dv.MoTa} {dv.ThanhPhanHoSo}";
                corpus.Add((dv.KyHieuDichVu, textMoTa, dv.TenDichVu));
            }

            // Tiền xử lý câu hỏi của sinh viên
            var userTokens = XuLyVanBanVaTrichXuatTokens(cauHoiNguoiDung);
            if (!userTokens.Any()) return new List<NlpSearchResult>();

            // Tách tokens cho toàn bộ tập dữ liệu mẫu
            var tokenizedCorpus = corpus.Select(c => (
                c.KyHieu,
                c.RawSample,
                Tokens: XuLyVanBanVaTrichXuatTokens(c.Text)
            )).ToList();

            var allWords = tokenizedCorpus.SelectMany(x => x.Tokens)
                                         .Concat(userTokens)
                                         .Distinct()
                                         .ToList();

            int N = tokenizedCorpus.Count + 1;

            // Tính IDF theo công thức Slide 3: IDF = ln((N+1)/(docCount+1)) + 1
            var idfDict = new Dictionary<string, double>();
            foreach (var word in allWords)
            {
                int docCount = tokenizedCorpus.Count(c => c.Tokens.Contains(word));
                if (userTokens.Contains(word)) docCount++;
                idfDict[word] = Math.Log((double)(N + 1) / (docCount + 1)) + 1.0;
            }

            // Tính vector TF-IDF cho câu hỏi người dùng
            var userVector = TinhTfIdfVector(userTokens, allWords, idfDict);
            var ketQuaKhop = new List<(string KyHieu, double Score, string MatchedSample)>();

            // So khớp Cosine Similarity với từng mẫu
            foreach (var doc in tokenizedCorpus)
            {
                var docVector = TinhTfIdfVector(doc.Tokens, allWords, idfDict);
                double score = TinhCosineSimilarity(userVector, docVector);

                if (score > 0.05)
                {
                    ketQuaKhop.Add((doc.KyHieu, score, doc.RawSample));
                }
            }

            var danhSachKetQua = new List<NlpSearchResult>();
            var grouped = ketQuaKhop.GroupBy(k => k.KyHieu);

            foreach (var grp in grouped)
            {
                var maxItem = grp.OrderByDescending(x => x.Score).First();
                var dv = dichVuList.FirstOrDefault(d => d.KyHieuDichVu == grp.Key);
                if (dv != null)
                {
                    double percentScore = Math.Round(maxItem.Score * 100, 1);
                    danhSachKetQua.Add(new NlpSearchResult
                    {
                        DichVu = dv,
                        DiemTuongDong = percentScore > 99.0 ? 99.0 : percentScore,
                        CauHoiKhopNhat = maxItem.MatchedSample,
                        LyDoGoiY = $"Khớp ngữ nghĩa với mẫu: \"{maxItem.MatchedSample}\""
                    });
                }
            }

            return danhSachKetQua.OrderByDescending(x => x.DiemTuongDong).ToList();
        }

        private List<string> XuLyVanBanVaTrichXuatTokens(string text)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();

            text = text.ToLowerInvariant();

            // Chuẩn hóa dấu gạch ngang (ví dụ gdqp-an thành gdqp an)
            text = text.Replace("-", " ");

            // Thay thế từ viết tắt / từ đồng nghĩa
            foreach (var kvp in TuDongNghia)
            {
                text = Regex.Replace(text, $@"\b{Regex.Escape(kvp.Key)}\b", kvp.Value);
            }

            // Giữ lại ký tự chữ cái tiếng Việt và số
            text = Regex.Replace(text, @"[^\p{L}\p{N}\s]", " ");

            // Lọc Stopwords và tách từ
            var rawWords = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                               .Where(w => !StopWords.Contains(w) && w.Length > 1)
                               .ToList();

            // Trích xuất Unigrams và Bigrams ghép từ (Slide 3 & 5)
            var tokens = new List<string>(rawWords);
            for (int i = 0; i < rawWords.Count - 1; i++)
            {
                tokens.Add($"{rawWords[i]}_{rawWords[i + 1]}");
            }

            return tokens;
        }

        private double[] TinhTfIdfVector(List<string> tokens, List<string> vocab, Dictionary<string, double> idfDict)
        {
            var vector = new double[vocab.Count];
            if (!tokens.Any()) return vector;

            var tfDict = tokens.GroupBy(t => t).ToDictionary(g => g.Key, g => (double)g.Count() / tokens.Count);

            for (int i = 0; i < vocab.Count; i++)
            {
                string word = vocab[i];
                if (tfDict.TryGetValue(word, out double tf))
                {
                    vector[i] = tf * idfDict[word];
                }
            }

            return vector;
        }

        private double TinhCosineSimilarity(double[] vecA, double[] vecB)
        {
            double dotProduct = 0.0;
            double normA = 0.0;
            double normB = 0.0;

            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                normA += vecA[i] * vecA[i];
                normB += vecB[i] * vecB[i];
            }

            if (normA == 0.0 || normB == 0.0) return 0.0;
            return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }
    }
}
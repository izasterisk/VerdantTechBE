# 📝 SEEDER.SQL - DOCUMENTATION & TROUBLESHOOTING GUIDE

**Ngày tạo:** 2025-11-20  
**Phiên bản:** v9.2  
**Tổng số products:** 173  
**Tổng số vendors:** 20  

---

## 🎯 TỔNG QUAN NHỮNG GÌ ĐÃ LÀM

### 1. Mục Tiêu
Tạo file `SEEDER.sql` hoàn chỉnh với 173 products từ file CSV, match 100% với **Entity Framework Code First** models.

### 2. Quy Trình Thực Hiện

#### Bước 1: Parse CSV File
- **Input:** `Products_v2.csv` (173 products)
- **Parsing:** Đọc và extract thông tin:
  - Category ID & Name (cha + con)
  - Product Name
  - Image URLs (front + back)

**⚠️ Lưu ý đặc biệt về Category Structure:**
- Nếu `category_id === sub_id` → Category KHÔNG có sub, products gắn vào **parent**
- Ví dụ: Category 11 "TÀI LIỆU HƯỚNG DẪN" có `11;TÀI LIỆU HƯỚNG DẪN;11;TÀI LIỆU HƯỚNG DẪN`
  - Chỉ tạo parent (ID 11)
  - KHÔNG tạo sub-category
  - Products gắn trực tiếp vào ID 11

#### Bước 2: Generate SQL Sections
Tool sử dụng: **Node.js script** (`generate_seeder.js`)

Các phần được generate:
1. ✅ **Vendor Profiles** (20 vendors)
   - Company names với encoding UTF-8
   - Slug (URL-friendly)
   - Business Registration Numbers
   
2. ✅ **User Bank Accounts** (20 accounts)
   - Bank code: `970436`
   - Account number: `1045069359`
   
3. ✅ **Wallets** (20 wallets)
   - Initial balance: 10,000,000 VNĐ

4. ✅ **Vendor Certificates** (20 certs)
   - Loại chứng chỉ: TCVN_7259, TCVN_8956, ISO_14001, VIETGAP, USDA_ORGANIC
   - Status: verified

5. ✅ **Product Categories** (40 total)
   - 11 parent categories
   - 29 sub-categories
   - Products gắn vào:
     - **Sub-categories** (nếu có sub)
     - **Parent category** (nếu không có sub, như category 11)

6. ✅ **Products** (173 products)
   - Giá: 1000 VNĐ (tất cả)
   - Commission rate: 10%
   - Stock quantity: 100
   - Energy efficiency rating: 1-5 (random based on product name hash)
   - Specifications: JSON (tự động generate theo loại sản phẩm)
   - Weight & Dimensions: Realistic dựa vào product type
   - Warranty: 0-24 tháng tùy loại

7. ✅ **Product Media Links** (176 images)
   - Front images: 173
   - Back images: 3

8. ✅ **Product Certificates** (41 certs - CHỈ BẮT BUỘC)
   Theo `agriculture_products.md`:
   - **GIONG_CAY** (8): Hạt giống rau (sub-category 3.1)
   - **CERT_CR** (19): Máy móc/thiết bị điện (4.3, 4.4, 5, 6.1)
   - **TCVN_TEST** (4): Bộ test nhanh (7.1)
   - **TCVN_BAO_HO** (5): Bảo hộ lao động (7.2)
   - **QCVN_BYT** (5): Bao bì sinh học tiếp xúc thực phẩm (9.1)

9. ✅ **Batch Inventory** (173 records)
   - SKU, batch number, lot number
   - Expiry date: chỉ cho phân bón/hạt giống/thuốc BVTV
   - Quality check: passed

10. ✅ **Product Serials** (160 serials - CHỈ MÁY MÓC)
    Máy móc cần serial number:
    - Máy xới đất, máy phun thuốc, máy gieo hạt
    - Máy bơm nước (tất cả loại)
    - Đèn bẫy côn trùng
    - Trạm thời tiết
    - Relay, Bộ điều khiển, Van điện từ
    
    Mỗi máy: **10 serial numbers** (2 sold, 8 stock)

---

## ⚠️ VẤN ĐỀ ĐÃ GẶP VÀ CÁCH FIX

### 🔴 VẤN ĐỀ 1: ENUM MediaPurpose Không Khớp

**Lỗi MySQL:**
```
Error Code: 1265. Data truncated for column 'purpose' at row 1
```

**Nguyên nhân:**
- Database dùng **Code First** (Entity Framework)
- Enum values trong database được map qua `MediaLinkConfiguration.cs`
- SEEDER ban đầu dùng sai enum values (PascalCase thay vì lowercase/camelCase)

**Chi tiết C# → Database Mapping:**

Xem file: `DAL/Data/Configurations/MediaLinkConfiguration.cs` (dòng 37-55)

```csharp
// C# Enum → Database String
MediaPurpose.Front                => "front"                   // lowercase
MediaPurpose.Back                 => "back"                    // lowercase
MediaPurpose.None                 => "none"                    // lowercase
MediaPurpose.ProductCertificatePdf => "productcertificatepdf" // lowercase, KHÔNG dấu
MediaPurpose.VendorCertificatesPdf => "vendorCertificatesPdf" // camelCase (C viết HOA)
```

**⚠️ CHÚ Ý Quan Trọng:**
- `vendorCertificatesPdf` có chữ **"C" viết hoa** ở giữa (camelCase)
- `productcertificatepdf` toàn bộ **lowercase**, không dấu
- `front` và `back` phải **lowercase**

**Cách Fix:**

1. **Trong SEEDER.sql**, enum values PHẢI là:
```sql
-- ❌ SAI (PascalCase)
'Front', 'Back', 'ProductCertificatePdf', 'VendorCertificatesPdf'

-- ✅ ĐÚNG (theo C# mapping)
'front', 'back', 'productcertificatepdf', 'vendorCertificatesPdf'
```

2. **Trong Schema SQL**, ENUM definition PHẢI là:
```sql
purpose ENUM('front', 'back', 'none', 'productcertificatepdf', 'vendorCertificatesPdf')
```

3. **Fix trong generate_seeder.js:**
```javascript
// Line ~80: Vendor Certificate PDFs
'vendorCertificatesPdf'  // ✅ camelCase với C viết hoa

// Line ~320: Product Certificate PDFs  
'productcertificatepdf'  // ✅ lowercase không dấu

// Line ~240: Product images
'front'  // ✅ lowercase
'back'   // ✅ lowercase
```

---

### 🔴 VẤN ĐỀ 2: Encoding UTF-8 Bị Lỗi

**Triệu chứng:**
```
Công Ty Phân Bón  →  C�ng Ty Ph�n B�n (hiển thị sai)
```

**Nguyên nhân:**
- File được append qua PowerShell terminal mà không chỉ định encoding UTF-8
- Hoặc file gốc đã bị encoding sai từ trước

**Cách Fix:**

1. **Đảm bảo Node.js đọc/ghi UTF-8:**
```javascript
fs.readFileSync('file.sql', {encoding: 'utf8'})
fs.writeFileSync('file.sql', content, 'utf8')
```

2. **Khi dùng PowerShell:**
```powershell
Get-Content file.sql -Encoding UTF8
Set-Content file.sql -Encoding UTF8
```

3. **Verify encoding:**
```powershell
Get-Content SEEDER.sql -Encoding UTF8 | Select-Object -First 20
```

Phải thấy tiếng Việt hiển thị đúng: `Công Ty Phân Bón Xanh Việt`

---

## 🛠️ HƯỚNG DẪN RE-GENERATE FILE SEEDER.SQL

### Điều Kiện Tiên Quyết:
- ✅ Node.js installed (version 14+)
- ✅ File `Products_v2.csv` trong thư mục `DB/`
- ✅ File `SEEDER_BACKUP.sql` (phần base: addresses, users, farms)

### Các Bước Re-Generate:

#### 1. Tạo SEEDER_BACKUP.sql (nếu chưa có):
```powershell
cd DB
Get-Content SEEDER.sql -Encoding UTF8 -TotalCount 248 | Set-Content SEEDER_BACKUP.sql -Encoding UTF8
```

Phần base (248 dòng đầu) bao gồm:
- Addresses (system + farms)
- Users (admin, staff, farmers, vendors)
- User Addresses
- Farm Profiles + Crops

#### 2. Chạy Script Generate:
```powershell
node generate_seeder.js
```

#### 3. Verify Output:
```powershell
# Kiểm tra số dòng
(Get-Content SEEDER.sql -Encoding UTF8).Count
# Kết quả: ~1239 dòng

# Kiểm tra encoding
Get-Content SEEDER.sql -Encoding UTF8 | Select-Object -Skip 255 -First 5
# Phải thấy: Công Ty Phân Bón... (không phải C�ng Ty)

# Kiểm tra enum values
Get-Content SEEDER.sql -Encoding UTF8 | Select-String "'vendorCertificatesPdf'"
# Phải thấy 20 matches
```

---

## 🚨 TROUBLESHOOTING GUIDE

### Lỗi 1: "Data truncated for column 'purpose'"

**Nguyên nhân:**
- Enum `purpose` trong SEEDER.sql không match với database

**Cách kiểm tra:**
```sql
-- Trong MySQL, check current ENUM definition:
SHOW CREATE TABLE media_links;
```

**Cách fix:**

1. Xem C# mapping trong `MediaLinkConfiguration.cs` (dòng 37-55)
2. Update `generate_seeder.js` để match
3. Re-generate SEEDER.sql
4. Nếu cần, ALTER TABLE:
```sql
ALTER TABLE media_links 
MODIFY purpose ENUM('front', 'back', 'none', 'productcertificatepdf', 'vendorCertificatesPdf') 
DEFAULT 'none';
```

---

### Lỗi 2: "Duplicate entry for key 'PRIMARY'"

**Nguyên nhân:**
- Database đã có data cũ với cùng ID

**Cách fix:**

Option 1: **Xóa data cũ trước** (RECOMMENDED):
```sql
SET FOREIGN_KEY_CHECKS = 0;

DELETE FROM product_serials;
DELETE FROM export_inventory;
DELETE FROM batch_inventory;
DELETE FROM product_certificates;
DELETE FROM product_reviews;
DELETE FROM products;
DELETE FROM product_categories;
DELETE FROM vendor_certificates;
DELETE FROM vendor_profiles;
DELETE FROM wallets;
DELETE FROM user_bank_accounts;

SET FOREIGN_KEY_CHECKS = 1;

-- Sau đó import SEEDER.sql
```

Option 2: **Reset AUTO_INCREMENT:**
```sql
ALTER TABLE products AUTO_INCREMENT = 1;
ALTER TABLE product_categories AUTO_INCREMENT = 1;
-- ... cho tất cả các tables
```

---

### Lỗi 3: "Cannot add foreign key constraint"

**Nguyên nhân:**
- Thứ tự import sai (child table trước parent table)
- Reference ID không tồn tại

**Cách kiểm tra:**
```sql
-- Check vendor_id có tồn tại không?
SELECT id FROM users WHERE id BETWEEN 17 AND 36 AND role = 'vendor';

-- Check category_id có tồn tại không?
SELECT id FROM product_categories WHERE id BETWEEN 12 AND 41;
```

**Cách fix:**
- Đảm bảo import đúng thứ tự trong SEEDER.sql:
  1. addresses
  2. users
  3. user_addresses
  4. farm_profiles, crops
  5. vendor_profiles, user_bank_accounts, wallets
  6. vendor_certificates
  7. product_categories
  8. products
  9. media_links
  10. product_certificates
  11. batch_inventory
  12. product_serials

---

### Lỗi 4: "Incorrect JSON value" (cho specifications/dimensions)

**Nguyên nhân:**
- JSON string không valid
- JSON có escape characters sai

**Ví dụ:**
```sql
-- ❌ SAI
specifications = '{"power":"125W","engine":"Electric"}'  -- Thiếu escape

-- ✅ ĐÚNG (trong SEEDER.sql)
specifications = "{\"power\":\"125W\",\"engine\":\"Electric\"}"
```

**Cách fix:**
- Trong `generate_seeder.js`, hàm `generateSpecs()` đã tự động escape quotes
- Nếu manual edit, dùng: `JSON.stringify(specs).replace(/"/g, '\\"')`

---

## 📊 CẤU TRÚC DỮ LIỆU CHI TIẾT

### Product Categories Hierarchy

```
1. DINH DƯỠNG THỰC VẬT (ID: 1)
   ├── 1.1 Phân Bón Hữu Cơ (ID: 12)
   ├── 1.2 Phân Bón Vi Sinh (ID: 13)
   ├── 1.3 Phân Bón Hữu Cơ Vi Sinh (ID: 14)
   ├── 1.4 Chất Cải Tạo Đất (ID: 15)
   └── 1.5 Chất Kích Thích Sinh Trưởng Sinh Học (ID: 16)

2. BẢO VỆ THỰC VẬT SINH HỌC (ID: 2)
   ├── 2.1 Thuốc Trừ Sâu Sinh Học (ID: 17)
   ├── 2.2 Thuốc Trừ Bệnh Sinh Học (ID: 18)
   └── 2.3 Thiết Bị Bẫy & Giám Sát Dịch Hại (ID: 19)

3. GIỐNG CÂY TRỒNG & VẬT LIỆU NHÂN GIỐNG (ID: 3)
   ├── 3.1 Hạt Giống Rau (ID: 20) ⚠️ BẮT BUỘC CERT
   └── 3.2 Vật Tư Ươm Giống (ID: 21)

4. HỆ THỐNG TƯỚI TIÊU (ID: 4)
   ├── 4.1 Tưới Nhỏ Giọt (ID: 22)
   ├── 4.2 Tưới Phun Mưa (ID: 23)
   ├── 4.3 Thiết Bị Bơm & Lọc Nước (ID: 24) ⚠️ BẮT BUỘC CERT
   ├── 4.4 Hệ Thống Điều Khiển Tưới (ID: 25) ⚠️ BẮT BUỘC CERT
   └── 4.5 Phụ Kiện Tưới Tiêu (ID: 26)

5. CÔNG NGHỆ & THIẾT BỊ THÔNG MINH (ID: 5) ⚠️ BẮT BUỘC CERT
   ├── 5.1 Thiết Bị Giám Sát (ID: 27)
   └── 5.2 Hệ Thống IoT & Tự Động Hóa (ID: 28)

6. MÁY MÓC & DỤNG CỤ (ID: 6)
   ├── 6.1 Máy Móc Nhỏ (ID: 29) ⚠️ BẮT BUỘC CERT + SERIAL
   ├── 6.2 Dụng Cụ Cầm Tay (ID: 30)
   └── 6.3 Thiết Bị Đo Lường (ID: 31)

7. AN TOÀN LAO ĐỘNG & KIỂM TRA CƠ BẢN (ID: 7)
   ├── 7.1 Bộ Test Nhanh Cơ Bản (ID: 32) ⚠️ BẮT BUỘC CERT
   └── 7.2 Bảo Hộ Lao Động (ID: 33) ⚠️ BẮT BUỘC CERT

8. VẬT LIỆU PHỦ & CHE CHẮN (ID: 8)
   ├── 8.1 Màng Phủ (ID: 34)
   ├── 8.2 Lưới Che Chắn (ID: 35)
   └── 8.3 Vải Địa Kỹ Thuật & Phụ Kiện (ID: 36)

9. BAO BÌ & BẢO QUẢN (ID: 9)
   ├── 9.1 Bao Bì Sinh Học (ID: 37) ⚠️ BẮT BUỘC CERT (tiếp xúc thực phẩm)
   └── 9.2 Thiết Bị Bảo Quản & Sơ Chế Nhỏ (ID: 38)

10. VẬT LIỆU TRỒNG TRỌT & GIÁ ĐỠ (ID: 10)
    ├── 10.1 Giàn Trồng & Giá Đỡ (ID: 39)
    └── 10.2 Phụ Kiện Thủy Canh Cơ Bản (ID: 40)

11. TÀI LIỆU HƯỚNG DẪN (ID: 11)
    └── 11 TÀI LIỆU HƯỚNG DẪN (ID: 41)
```

### Vendor Distribution (173 products / 20 vendors)

Mỗi vendor bán ~9 products:
- Vendor 17 (User ID 17): Products 1-9
- Vendor 18 (User ID 18): Products 10-18
- ...
- Vendor 36 (User ID 36): Products 165-173

---

## 🔧 CÁCH CHỈNH SỬA NẾU CẦN

### 1. Thêm/Sửa Products Mới

**Thêm vào `Products_v2.csv`:**
```csv
Category_Father;Category_Name;Category_sons;Category_sons Name;Product Name;;image_url 1;image_url 2
1;DINH DƯỠNG THỰC VẬT;1.1;Phân Bón Hữu Cơ;Tên sản phẩm mới;;https://...;https://...
```

**Chạy lại:**
```powershell
node generate_seeder.js
```

### 2. Thay Đổi Giá/Commission/Stock

**Edit trong `generate_seeder.js` (line ~355):**
```javascript
const unitPrice = 1000;      // Thay đổi giá
const commission = 10.00;    // Thay đổi commission rate
const stock = 100;           // Thay đổi stock

rows.push(`(..., ${unitPrice}.00, ${commission}.00, ..., ${stock}, ...)`);
```

### 3. Thay Đổi Vendor Bank Account

**Edit trong `generate_seeder.js` (line ~280):**
```javascript
const bankCode = '970436';        // Thay bank code
const accountNumber = '1045069359'; // Thay account number
```

### 4. Thêm Loại Certificate Mới

**Edit trong `generate_seeder.js` (line ~425):**
```javascript
const certRequirements = {
    '1': 'TCVN_7259',      // Category ID → Cert Code
    '2': 'THUOC_BVTV',
    '3.1': 'GIONG_CAY',
    '4.3': 'CERT_CR',
    // Thêm mới:
    '12.5': 'NEW_CERT_CODE'
};

// Và thêm cert name:
case 'NEW_CERT_CODE':
    name = 'Tên chứng chỉ mới';
    break;
```

---

## 📋 CHECKLIST KHI IMPORT

Trước khi import `SEEDER.sql`, check:

### ✅ Pre-Import Checklist:

```powershell
# 1. Check file size (phải ~188-192 KB)
(Get-Item DB/SEEDER.sql).Length

# 2. Check số dòng (phải ~1239 dòng)
(Get-Content DB/SEEDER.sql -Encoding UTF8).Count

# 3. Check encoding (tiếng Việt đúng?)
Get-Content DB/SEEDER.sql -Encoding UTF8 | Select-Object -Skip 255 -First 5

# 4. Check enum purpose values
Get-Content DB/SEEDER.sql -Encoding UTF8 | Select-String "'vendorCertificatesPdf'" | Measure-Object
# Kết quả: 20

Get-Content DB/SEEDER.sql -Encoding UTF8 | Select-String "'productcertificatepdf'" | Measure-Object
# Kết quả: 41

Get-Content DB/SEEDER.sql -Encoding UTF8 | Select-String ", 'front'," | Measure-Object
# Kết quả: 173

# 5. Check product count
Get-Content DB/SEEDER.sql -Encoding UTF8 | Select-String "PRD-C\d+-\d+" | Measure-Object
# Kết quả: 173
```

### ✅ Import Command:

```sql
-- Method 1: MySQL CLI
mysql -u root -p verdanttech_db < DB/SEEDER.sql

-- Method 2: MySQL Workbench
-- File → Run SQL Script → Chọn SEEDER.sql

-- Method 3: Via Code (nếu dùng migration)
-- dotnet ef database update
```

### ✅ Post-Import Verification:

```sql
-- Check vendors
SELECT COUNT(*) FROM vendor_profiles;  -- Phải = 20

-- Check products
SELECT COUNT(*) FROM products;  -- Phải = 173

-- Check categories
SELECT COUNT(*) FROM product_categories WHERE parent_id IS NULL;  -- Phải = 11
SELECT COUNT(*) FROM product_categories WHERE parent_id IS NOT NULL;  -- Phải = 30

-- Check media links
SELECT purpose, COUNT(*) 
FROM media_links 
GROUP BY purpose;
-- Kết quả:
-- front: 173
-- back: 3
-- vendorCertificatesPdf: 20
-- productcertificatepdf: 41

-- Check product serials (chỉ máy móc)
SELECT COUNT(*) FROM product_serials;  -- Phải = 160

-- Check batch inventory
SELECT COUNT(*) FROM batch_inventory;  -- Phải = 173
```

---

## 🎓 QUY TẮC QUAN TRỌNG KHI LÀM VIỆC VỚI CODE FIRST

### 1. Enum Values LUÔN Theo C# Configuration

**KHÔNG BAO GIỜ** tự đoán enum values trong SQL!

**Luôn kiểm tra:**
- `DAL/Data/Enums.cs` - Định nghĩa enum
- `DAL/Data/Configurations/` - Mapping C# → Database

**Ví dụ:**
```csharp
// Enums.cs
public enum MediaPurpose {
    Front,                    // C# PascalCase
    Back,
    ProductCertificatePdf
}

// MediaLinkConfiguration.cs (QUAN TRỌNG!)
MediaPurpose.Front => "front"  // ← Database lowercase!
```

### 2. String Encoding LUÔN UTF-8

- Node.js: `{encoding: 'utf8'}`
- PowerShell: `-Encoding UTF8`
- MySQL: `CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci`

### 3. JSON Fields Phải Escape Đúng

```javascript
// ❌ SAI
"{\"name\":\"value\"}"  // Single escape

// ✅ ĐÚNG  
"{\\\"name\\\":\\\"value\\\"}"  // Double escape cho SQL string
```

### 4. Foreign Keys Phải Tồn Tại Trước

Thứ tự import:
```
users → vendor_profiles → products → product_certificates
  ↓          ↓                ↓
addresses   wallets      batch_inventory
```

---

## 📁 FILES QUAN TRỌNG

### DB/ Folder:
- ✅ `SEEDER.sql` - **File chính để import** (1,239 dòng, 188 KB)
- ✅ `generate_seeder.js` - Script Node.js để re-generate
- ✅ `Products_v2.csv` - Data source (173 products)
- ✅ `verdanttech_schema_v9.2.sql` - Reference schema (đã update ENUM)
- ✅ `SEEDER_BACKUP.sql` - Base template (248 dòng: addresses, users, farms)
- ✅ `Notes.md` - File này (documentation)

### DAL/Data/ Folder (Code First Models):
- 📌 `Enums.cs` - Enum definitions
- 📌 `Configurations/MediaLinkConfiguration.cs` - **QUAN TRỌNG** cho enum mapping
- 📌 `Models/MediaLink.cs` - Model definition

---

## 🔍 VALIDATION QUERIES

Sau khi import, chạy để verify:

```sql
-- 1. Summary
SELECT 
    (SELECT COUNT(*) FROM products) AS total_products,
    (SELECT COUNT(*) FROM vendor_profiles) AS total_vendors,
    (SELECT COUNT(*) FROM product_categories) AS total_categories,
    (SELECT COUNT(*) FROM media_links) AS total_media,
    (SELECT COUNT(*) FROM product_certificates) AS total_prod_certs,
    (SELECT COUNT(*) FROM batch_inventory) AS total_batches;

-- 2. Products by Category (parent)
SELECT 
    pc_parent.name AS category_name,
    COUNT(p.id) AS product_count
FROM products p
JOIN product_categories pc_child ON p.category_id = pc_child.id
JOIN product_categories pc_parent ON pc_child.parent_id = pc_parent.id
GROUP BY pc_parent.id, pc_parent.name
ORDER BY pc_parent.id;

-- 3. Products by Vendor
SELECT 
    u.full_name AS vendor_name,
    COUNT(p.id) AS product_count
FROM products p
JOIN users u ON p.vendor_id = u.id
GROUP BY u.id, u.full_name
ORDER BY u.id;

-- 4. Media Links Breakdown
SELECT 
    owner_type,
    purpose,
    COUNT(*) AS count
FROM media_links
GROUP BY owner_type, purpose
ORDER BY owner_type, purpose;

-- 5. Products cần serial number (máy móc)
SELECT 
    p.id,
    p.product_name,
    COUNT(ps.id) AS serial_count
FROM products p
LEFT JOIN product_serials ps ON p.id = ps.product_id
WHERE ps.id IS NOT NULL
GROUP BY p.id, p.product_name
HAVING serial_count > 0;
```

---

## 🎯 QUICK REFERENCE

### Enum MediaPurpose Values (C# → Database):

| C# Enum | Database String | Usage |
|---------|----------------|-------|
| `MediaPurpose.Front` | `'front'` | Product front image |
| `MediaPurpose.Back` | `'back'` | Product back image |
| `MediaPurpose.None` | `'none'` | No specific purpose |
| `MediaPurpose.ProductCertificatePdf` | `'productcertificatepdf'` | Product cert PDF |
| `MediaPurpose.VendorCertificatesPdf` | `'vendorCertificatesPdf'` | Vendor cert PDF (⚠️ camelCase!) |

### Product Specifications Examples:

```json
// Phân bón
{"npk":"5-5-5","weight":"50kg","type":"Phân hữu cơ"}

// Máy móc
{"power":"125W","engine":"Electric","capacity":"25L"}

// Hạt giống
{"germination_rate":"90-95%","pack_size":"100g","type":"F1 Hybrid"}

// Thiết bị điện
{"voltage":"12V","interface":"Analog 4-20mA","range":"0-14 pH"}

// Ống/vật liệu
{"diameter":"16mm","length":"100m","material":"PE"}
```

### Warranty Months by Type:

| Product Type | Warranty (months) |
|--------------|-------------------|
| Phân bón, hạt giống, vật tư | 0 |
| Dụng cụ cầm tay | 0 |
| Đèn, bẫy côn trùng | 12 |
| Máy móc, thiết bị điện | 24 |

### Weight & Dimensions Guidelines:

| Product Type | Weight (kg) | Dimensions (cm) |
|--------------|-------------|-----------------|
| Hạt giống | 0.1 | 12×8×2 |
| Thuốc BVTV | 0.5 | 15×10×8 |
| Dụng cụ cầm tay | 0.8 | 35×10×5 |
| Phân bón (25kg) | 25.0 | 60×40×15 |
| Phân bón (50kg) | 50.0 | 80×50×20 |
| Máy bơm | 35.0 | 60×40×50 |
| Máy xới đất | 150.0 | 180×120×100 |
| Ống cuộn 100m | 12.0 | 100×30×30 |

---

## 🚀 TÓM TẮT NHANH

### Để Re-Generate SEEDER.sql:

```powershell
cd DB
node generate_seeder.js
```

### Để Verify Enum Values:

```powershell
Get-Content SEEDER.sql -Encoding UTF8 | Select-String "'vendorCertificatesPdf'" | Measure-Object
# = 20 ✅

Get-Content SEEDER.sql -Encoding UTF8 | Select-String "'productcertificatepdf'" | Measure-Object
# = 41 ✅

Get-Content SEEDER.sql -Encoding UTF8 | Select-String ", 'front'," | Measure-Object
# = 173 ✅
```

### Để Clean Database Trước Khi Re-Import:

```sql
SET FOREIGN_KEY_CHECKS = 0;
DELETE FROM product_serials;
DELETE FROM export_inventory;
DELETE FROM batch_inventory;
DELETE FROM product_certificates WHERE id <= 500;
DELETE FROM products WHERE id <= 500;
DELETE FROM product_categories WHERE id <= 100;
DELETE FROM vendor_certificates WHERE id <= 50;
DELETE FROM vendor_profiles WHERE id <= 50;
DELETE FROM wallets WHERE id <= 50;
DELETE FROM user_bank_accounts WHERE id <= 50;
DELETE FROM media_links WHERE id <= 1000;
SET FOREIGN_KEY_CHECKS = 1;
```

---

## 📞 CONTACTS & RESOURCES

- **C# Models:** `DAL/Data/Models/`
- **EF Configurations:** `DAL/Data/Configurations/`
- **Enum Definitions:** `DAL/Data/Enums.cs`
- **CSV Data Source:** `DB/Products_v2.csv`
- **Certificate Requirements:** `DB/agriculture_products.md`

---

**Last Updated:** 2025-11-20  
**Version:** v9.2  
**Status:** ✅ Production Ready


-- Lược đồ Cơ sở Dữ liệu VerdantTech Solutions
-- Nền tảng Thiết bị Nông nghiệp Xanh Tích hợp AI cho Trồng Rau Bền vững
-- Phiên bản: 10.1
-- Engine: InnoDB (hỗ trợ giao dịch)
-- Bộ ký tự: utf8mb4 (hỗ trợ đa ngôn ngữ)

-- Set timezone to UTC for all database operations
SET time_zone = '+00:00';
SET GLOBAL time_zone = '+00:00';

-- Tạo cơ sở dữ liệu
CREATE DATABASE IF NOT EXISTS verdanttech_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE verdanttech_db;

-- =====================================================
-- CÁC BẢNG QUẢN LÝ NGƯỜI DÙNG
-- =====================================================

-- Bảng địa chỉ chung cho tất cả các thực thể
CREATE TABLE addresses (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    location_address TEXT,
    province VARCHAR(100) NOT NULL,
    district VARCHAR(100) NOT NULL,
    commune VARCHAR(100) NOT NULL,
    province_code VARCHAR(20) NOT NULL COMMENT 'Mã tỉnh/thành phố theo hệ thống GoShip',
    district_code VARCHAR(20) NOT NULL COMMENT 'Mã quận/huyện theo hệ thống GoShip',
    commune_code VARCHAR(20) NOT NULL COMMENT 'Mã phường/xã theo hệ thống GoShip',
    latitude DECIMAL(10,8) NULL COMMENT 'Vĩ độ',
    longitude DECIMAL(11,8) NULL COMMENT 'Kinh độ',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng địa chỉ chung cho các thực thể';

CREATE TABLE users (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role ENUM('customer', 'staff', 'vendor', 'admin') NOT NULL DEFAULT 'customer',
    full_name VARCHAR(255) NOT NULL,
    phone_number VARCHAR(20),
    tax_code VARCHAR(100) UNIQUE,
    is_verified BOOLEAN DEFAULT FALSE,
    verification_token VARCHAR(255),
    verification_sent_at TIMESTAMP NULL,
    avatar_url VARCHAR(500),
    status ENUM('active', 'inactive', 'suspended', 'deleted') DEFAULT 'active',
    last_login_at TIMESTAMP NULL,
    RefreshToken VARCHAR(500),
    RefreshTokenExpiresAt TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP NULL,
    
    INDEX idx_email (email),
    INDEX idx_status (status),
    INDEX idx_created_at (created_at),
    INDEX idx_role_created (role, created_at) COMMENT 'Cho queries thống kê user mới theo role',
    INDEX idx_refresh_token (RefreshToken, RefreshTokenExpiresAt) COMMENT 'Cho refresh token validation'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng xác thực và hồ sơ người dùng cơ bản';

-- Bảng trung gian quản lý nhiều địa chỉ cho người dùng
CREATE TABLE user_addresses (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    address_id BIGINT UNSIGNED NOT NULL,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP NULL,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (address_id) REFERENCES addresses(id) ON DELETE RESTRICT,
    INDEX idx_user_deleted (user_id, is_deleted) COMMENT 'Optimized cho queries filter theo user và trạng thái',
    INDEX idx_address_deleted (address_id, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng trung gian quản lý nhiều địa chỉ cho người dùng';

-- Hồ sơ nhà cung cấp cho người bán
CREATE TABLE vendor_profiles (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL UNIQUE,
    company_name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    business_registration_number VARCHAR(100) UNIQUE,
    notes VARCHAR(255) NULL,
    subscription_active BOOLEAN DEFAULT FALSE COMMENT 'Trạng thái gói đăng ký/subscription của vendor',
    verified_at TIMESTAMP NULL,
    verified_by BIGINT UNSIGNED NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_company_name (company_name),
    INDEX idx_verified_pending (verified_at, verified_by) COMMENT 'Cho queries tìm vendor chờ xác minh'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Hồ sơ nhà cung cấp/người bán và chi tiết xác minh';

-- Thông tin chứng chỉ của nhà cung cấp
CREATE TABLE vendor_certificates (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL,
    certification_code VARCHAR(50) NOT NULL,
    certification_name VARCHAR(255) NOT NULL,
    status ENUM('pending', 'verified', 'rejected') DEFAULT 'pending',
    rejection_reason VARCHAR(500) NULL COMMENT 'Lý do từ chối nếu trạng thái bị từ chối',
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    verified_at TIMESTAMP NULL,
    verified_by BIGINT UNSIGNED NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_vendor_uploaded (vendor_id, uploaded_at) COMMENT 'Cho queries lấy certificates theo vendor, sort by uploaded_at',
    INDEX idx_vendor_status (vendor_id, status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Chứng chỉ bền vững do nhà cung cấp tải lên để xác minh';

-- Tài khoản ngân hàng của người dùng (user và vendor đều có thể có nhiều tài khoản ngân hàng)
CREATE TABLE user_bank_accounts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    bank_code VARCHAR(20) NOT NULL,
    account_number VARCHAR(50) NOT NULL,
    owner_name VARCHAR(50) NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    UNIQUE KEY unique_user_bank_account (user_id, account_number),
    INDEX idx_user_active (user_id, is_active) COMMENT 'Optimized cho queries filter bank account active của user'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Tài khoản ngân hàng của người dùng (cả customer và vendor)';

-- =====================================================
-- CÁC BẢNG DỮ LIỆU MÔI TRƯỜNG
-- =====================================================

-- Hồ sơ trang trại cho nông dân
CREATE TABLE farm_profiles (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    farm_name VARCHAR(255) NOT NULL,
    farm_size_hectares DECIMAL(10,2),
    address_id BIGINT UNSIGNED NULL,
    status ENUM('Active', 'Maintenance', 'Deleted') DEFAULT 'Active',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (address_id) REFERENCES addresses(id) ON DELETE RESTRICT,
    INDEX idx_user_status (user_id, status) COMMENT 'Cho queries filter farms theo user và status',
    INDEX idx_address (address_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Chi tiết hồ sơ trang trại cho người dùng nông dân';

-- Bảng cây trồng cho trang trại
CREATE TABLE crops (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    farm_profile_id BIGINT UNSIGNED NOT NULL,
    crop_name VARCHAR(255) NOT NULL COMMENT 'Tên loại cây trồng',
    planting_date DATE NOT NULL COMMENT 'Ngày trồng',
    planting_method ENUM('gieo_hat_truc_tiep', 'uom_trong_khay', 'cay_cay_con', 'sinh_san_sinh_duong', 'giam_canh') NULL COMMENT 'Phương pháp gieo trồng: gieo_hat_truc_tiep (Gieo hạt trực tiếp), uom_trong_khay (Ươm trong khay), cay_cay_con (Cấy cây con), sinh_san_sinh_duong (Sinh sản sinh dưỡng từ củ/thân), giam_canh (Giâm cành)',
    crop_type ENUM('rau_an_la', 'rau_an_qua', 'rau_cu', 'rau_thom') NULL COMMENT 'Loại cây trồng: rau_an_la (Rau ăn lá như rau muống, cải, xà lách), rau_an_qua (Rau ăn quả như cà chua, dưa leo, ớt), rau_cu (Củ như cà rốt, củ cải), rau_thom (Rau thơm như húng, ngò)',
    farming_type ENUM('tham_canh', 'luan_canh', 'xen_canh', 'nha_luoi', 'thuy_canh') NULL COMMENT 'Loại hình canh tác: tham_canh (Thâm canh), luan_canh (Luân canh), xen_canh (Xen canh), nha_luoi (Nhà lưới/nhà màng), thuy_canh (Thủy canh)',
    status ENUM('growing', 'harvested', 'failed', 'deleted') DEFAULT 'growing' COMMENT 'Trạng thái của cây trồng: growing (Đang sinh trưởng), harvested (Đã thu hoạch), failed (Thất bại), deleted (Đã xóa)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (farm_profile_id) REFERENCES farm_profiles(id) ON DELETE RESTRICT,
    INDEX idx_farm_status (farm_profile_id, status) COMMENT 'Optimized cho queries filter crops theo farm và status',
    INDEX idx_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Quản lý cây trồng của trang trại (một trang trại có nhiều cây trồng)';

-- Dữ liệu giám sát môi trường
CREATE TABLE environmental_data (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    farm_profile_id BIGINT UNSIGNED NOT NULL,
    customer_id BIGINT UNSIGNED NOT NULL,
    measurement_start_date DATE NOT NULL COMMENT 'Ngày bắt đầu ghi nhận dữ liệu',
    measurement_end_date DATE NOT NULL COMMENT 'Ngày kết thúc ghi nhận dữ liệu',
    sand_pct   DECIMAL(5,2) NULL COMMENT 'Sand (%) 0–30 cm',
    silt_pct   DECIMAL(5,2) NULL COMMENT 'Silt (%) 0–30 cm',
    clay_pct   DECIMAL(5,2) NULL COMMENT 'Clay (%) 0–30 cm',
    phh2o      DECIMAL(4,2) NULL CHECK (phh2o >= 0 AND phh2o <= 14) COMMENT 'pH (H2O) 0–30 cm',    
    precipitation_sum DECIMAL(7,2) NULL COMMENT 'Tổng lượng mưa (mm)',
    et0_fao_evapotranspiration DECIMAL(7,2) NULL COMMENT 'Lượng thoát hơi nước/bốc hơi(mm)',
    co2_footprint DECIMAL(10,2) COMMENT 'Lượng khí thải CO2 tính bằng kg',
    notes VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (farm_profile_id) REFERENCES farm_profiles(id) ON DELETE RESTRICT,
    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_farm_dates (farm_profile_id, measurement_start_date, measurement_end_date),
    INDEX idx_farm_profile (farm_profile_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Dữ liệu môi trường do nông dân nhập thủ công';

-- Theo dõi việc sử dụng phân bón
CREATE TABLE fertilizers (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    environmental_data_id BIGINT UNSIGNED NOT NULL UNIQUE,
    organic_fertilizer DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Phân hữu cơ (kg)',
    npk_fertilizer DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Phân NPK tổng hợp (kg)',
    urea_fertilizer DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Phân urê (kg)', 
    phosphate_fertilizer DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Phân lân (kg)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (environmental_data_id) REFERENCES environmental_data(id) ON DELETE RESTRICT,
    UNIQUE KEY uk_fertilizers_environmental_data (environmental_data_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Dữ liệu sử dụng phân bón để tính toán lượng khí thải CO2 (quan hệ 1-1 với environmental_data)';

-- Theo dõi việc sử dụng năng lượng
CREATE TABLE energy_usage (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    environmental_data_id BIGINT UNSIGNED NOT NULL UNIQUE,
    electricity_kwh DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Điện tiêu thụ (kWh)',
    gasoline_liters DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Xăng sử dụng (lít)',
    diesel_liters DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Dầu diesel sử dụng (lít)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,  

    FOREIGN KEY (environmental_data_id) REFERENCES environmental_data(id) ON DELETE RESTRICT,
    UNIQUE KEY uk_energy_usage_environmental_data (environmental_data_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Dữ liệu sử dụng năng lượng để tính toán lượng khí thải CO2 (quan hệ 1-1 với environmental_data)';

-- =====================================================
-- CÁC BẢNG ĐÁNH GIÁ ĐỘ BỀN VỮNG TRANG TRẠI (SUSTAINABILITY SURVEY)
-- =====================================================

-- Bảng câu trả lời của người dùng
CREATE TABLE survey_responses (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    farm_profile_id BIGINT UNSIGNED NULL COMMENT 'Trang trại được đánh giá',
    question_id BIGINT UNSIGNED NOT NULL COMMENT 'Câu hỏi được trả lời',    
    text_answer TEXT NULL COMMENT 'Câu trả lời dạng text - bắt buộc nếu question_type = text',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (farm_profile_id) REFERENCES farm_profiles(id) ON DELETE CASCADE,
    INDEX idx_farm_question (farm_profile_id, question_id) COMMENT 'Optimized cho queries ORDER BY question_id'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng lưu câu trả lời đánh giá độ bền vững của người dùng';

-- =====================================================
-- CÁC BẢNG CHATBOT AI
-- =====================================================

-- Các cuộc hội thoại chatbot
CREATE TABLE chatbot_conversations (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    customer_id BIGINT UNSIGNED NOT NULL,
    session_id VARCHAR(255) NOT NULL COMMENT 'UUID session identifier',
    title VARCHAR(255),
    context TEXT COMMENT 'Bối cảnh cuộc hội thoại và metadata',
    is_active BOOLEAN DEFAULT TRUE,
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_customer_active_started (customer_id, is_active, started_at) COMMENT 'Optimized cho pagination với filter is_active',
    INDEX idx_session_id (session_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Các phiên hội thoại chatbot';

-- Tin nhắn chatbot
CREATE TABLE chatbot_messages (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    conversation_id BIGINT UNSIGNED NOT NULL,
    message_type ENUM('user', 'bot', 'system') NOT NULL,
    message_text TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, 

    FOREIGN KEY (conversation_id) REFERENCES chatbot_conversations(id) ON DELETE RESTRICT,
    INDEX idx_conversation_created (conversation_id, created_at) COMMENT 'Optimized cho ORDER BY created_at'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Các tin nhắn chatbot riêng lẻ';

-- =====================================================
-- CÁC BẢNG CHAT GIỮA CUSTOMER VÀ VENDOR
-- =====================================================

-- Các cuộc hội thoại giữa customer và vendor
CREATE TABLE customer_vendor_conversations (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    customer_id BIGINT UNSIGNED NOT NULL,
    vendor_id BIGINT UNSIGNED NOT NULL,
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_message_at TIMESTAMP NULL COMMENT 'Thời điểm tin nhắn cuối cùng - dùng để sắp xếp',

    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE RESTRICT,
    UNIQUE INDEX idx_customer_vendor (customer_id, vendor_id) COMMENT 'Đảm bảo một cặp customer-vendor chỉ có 1 conversation',
    INDEX idx_customer_last (customer_id, last_message_at) COMMENT 'Optimized cho pagination theo customer sort by last_message_at',
    INDEX idx_vendor_last (vendor_id, last_message_at) COMMENT 'Optimized cho pagination theo vendor sort by last_message_at'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Các cuộc hội thoại giữa customer và vendor';

-- Tin nhắn trong cuộc hội thoại customer-vendor
CREATE TABLE customer_vendor_messages (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    conversation_id BIGINT UNSIGNED NOT NULL,
    product_id BIGINT UNSIGNED NULL COMMENT 'Sản phẩm được đề cập trong tin nhắn (nếu có)',
    sender_type ENUM('customer', 'vendor') NOT NULL COMMENT 'Người gửi: customer hoặc vendor',
    message_text TEXT NOT NULL,
    is_read BOOLEAN DEFAULT FALSE COMMENT 'Tin nhắn đã được đọc chưa - để tracking unread messages',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (conversation_id) REFERENCES customer_vendor_conversations(id) ON DELETE RESTRICT,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    INDEX idx_conversation_created (conversation_id, created_at) COMMENT 'Optimized cho ORDER BY created_at khi load tin nhắn',
    INDEX idx_conversation_unread (conversation_id, is_read) COMMENT 'Optimized cho queries đếm unread messages'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Các tin nhắn trong cuộc hội thoại giữa customer và vendor';

-- =====================================================
-- CÁC BẢNG CỘNG ĐỒNG
-- =====================================================

-- Danh mục diễn đàn
CREATE TABLE forum_categories (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,  

    INDEX idx_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Các danh mục thảo luận diễn đàn';

-- Bài viết diễn đàn
CREATE TABLE forum_posts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    forum_category_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    content JSON NOT NULL COMMENT 'Các khối nội dung hỗn hợp: [{"order": 1, "type": "text", "content": "Hello world"}, {"order": 2, "type": "image", "content": "1 (là id từ bảng MediaLink)"}]',
    tags VARCHAR(500) COMMENT 'Thẻ, danh sách phân cách bằng dấu phẩy',
    view_count BIGINT DEFAULT 0,
    like_count INT DEFAULT 0,
    dislike_count INT DEFAULT 0,
    is_pinned BOOLEAN DEFAULT FALSE,
    status ENUM('visible', 'hidden') DEFAULT 'visible',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (forum_category_id) REFERENCES forum_categories(id) ON DELETE RESTRICT,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_category_created (forum_category_id, created_at) COMMENT 'Optimized cho pagination theo category',
    INDEX idx_user (user_id),
    INDEX idx_slug (slug),
    INDEX idx_status_created (status, created_at) COMMENT 'Optimized cho filter status + sort created_at'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bài viết thảo luận diễn đàn';

-- Bình luận diễn đàn
CREATE TABLE forum_comments (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    forum_post_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    parent_id BIGINT UNSIGNED NULL COMMENT 'Dành cho bình luận lồng nhau',
    content TEXT NOT NULL,
    like_count INT DEFAULT 0,
    dislike_count INT DEFAULT 0,
    status ENUM('visible', 'moderated', 'deleted') DEFAULT 'visible',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (forum_post_id) REFERENCES forum_posts(id) ON DELETE RESTRICT,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (parent_id) REFERENCES forum_comments(id) ON DELETE RESTRICT,
    INDEX idx_post_parent_created (forum_post_id, parent_id, created_at) COMMENT 'Optimized cho queries filter parent + sort created',
    INDEX idx_user (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bình luận bài viết diễn đàn';

-- =========================
-- PRODUCT & REGISTRATION
-- =========================

-- Danh mục sản phẩm
CREATE TABLE product_categories (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    parent_id BIGINT UNSIGNED NULL,
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    serial_required BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (parent_id) REFERENCES product_categories(id) ON DELETE RESTRICT,
    INDEX idx_parent (parent_id),
    INDEX idx_slug (slug),
    INDEX idx_active_parent (is_active, parent_id) COMMENT 'Optimized cho queries filter active categories'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Danh mục sản phẩm theo cấp bậc';

CREATE TABLE product_registrations (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL,
    category_id BIGINT UNSIGNED NOT NULL,
    proposed_product_code VARCHAR(100) UNIQUE NOT NULL,
    proposed_product_name VARCHAR(255) NOT NULL,
    description TEXT,
    unit_price DECIMAL(12,2) NOT NULL COMMENT 'Đơn giá sản phẩm đề xuất',
    energy_efficiency_rating DECIMAL(3,1) CHECK (energy_efficiency_rating >= 0 AND energy_efficiency_rating <= 5) COMMENT 'Xếp hạng hiệu suất năng lượng (0-5)',
    specifications JSON COMMENT 'Thông số kỹ thuật dưới dạng cặp khóa-giá trị',
    manual_urls VARCHAR(1000) COMMENT 'URL hướng dẫn/sổ tay, phân cách bằng dấu phẩy',
    public_url VARCHAR(500) COMMENT 'URL công khai cho manual files',
    warranty_months INT DEFAULT 12,
    weight_kg DECIMAL(10,3) NOT NULL COMMENT 'Trọng lượng sản phẩm (kg) - bắt buộc',
    dimensions_cm JSON COMMENT '{chiều dài, chiều rộng, chiều cao}',
    status ENUM('pending', 'approved', 'rejected') DEFAULT 'pending',
    rejection_reason VARCHAR(500) NULL,
    approved_by BIGINT UNSIGNED NULL,
    approved_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (category_id) REFERENCES product_categories(id) ON DELETE RESTRICT,
    FOREIGN KEY (approved_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_vendor_created (vendor_id, created_at) COMMENT 'Optimized cho pagination theo vendor',
    INDEX idx_vendor_status (vendor_id, status),
    INDEX idx_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Product registrations';

-- Bảng sản phẩm
CREATE TABLE products (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    category_id BIGINT UNSIGNED NOT NULL,
    vendor_id BIGINT UNSIGNED NOT NULL,
    product_code VARCHAR(100) UNIQUE NOT NULL,
    product_name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    description TEXT,
    unit_price DECIMAL(12,2) NOT NULL COMMENT 'Đơn giá sản phẩm ',
    commission_rate DECIMAL(5,2) DEFAULT 0.00 COMMENT 'Tỷ lệ hoa hồng cho nhà cung cấp',
    discount_percentage DECIMAL(5,2) DEFAULT 0.00,
    energy_efficiency_rating INT CHECK (energy_efficiency_rating >= 0 AND energy_efficiency_rating <= 5) COMMENT 'Xếp hạng hiệu suất năng lượng (0-5)',
    specifications JSON COMMENT 'Thông số kỹ thuật dưới dạng cặp khóa-giá trị',
    manual_urls VARCHAR(1000) COMMENT 'URL hướng dẫn/sổ tay, phân cách bằng dấu phẩy',
    public_url VARCHAR(500) COMMENT 'URL công khai cho manual files',
    warranty_months INT DEFAULT 12,
    stock_quantity INT DEFAULT 0,
    weight_kg DECIMAL(10,3) NOT NULL COMMENT 'Trọng lượng sản phẩm (kg) - bắt buộc',
    dimensions_cm JSON COMMENT '{chiều dài, chiều rộng, chiều cao}',
    is_active BOOLEAN DEFAULT TRUE,
    view_count BIGINT DEFAULT 0,
    sold_count BIGINT DEFAULT 0,
    rating_average DECIMAL(3,2) DEFAULT 0.00,
    registration_id BIGINT UNSIGNED NULL COMMENT 'Khóa ngoại đến product_registrations',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (category_id) REFERENCES product_categories(id) ON DELETE RESTRICT,
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (registration_id) REFERENCES product_registrations(id) ON DELETE RESTRICT,
    INDEX idx_category_created (category_id, created_at) COMMENT 'Optimized cho pagination theo category',
    INDEX idx_vendor_created (vendor_id, created_at) COMMENT 'Optimized cho pagination theo vendor',
    INDEX idx_vendor_active (vendor_id, is_active) COMMENT 'Cho queries filter active products của vendor',
    INDEX idx_active_stock (is_active, stock_quantity) COMMENT 'Cho queries tìm out of stock / low stock products',
    INDEX idx_product_code (product_code),
    INDEX idx_slug (slug),
    INDEX idx_registration (registration_id),
    FULLTEXT idx_search (product_name, description)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Sản phẩm thiết bị nông nghiệp xanh trong kho công ty';


-- Bảng lưu trữ snapshot lịch sử thay đổi sản phẩm
CREATE TABLE product_snapshot (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL COMMENT 'ID sản phẩm tham chiếu',
    category_id BIGINT UNSIGNED NOT NULL,
    vendor_id BIGINT UNSIGNED NOT NULL,
    product_code VARCHAR(100) NOT NULL COMMENT 'Mã sản phẩm',
    product_name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL COMMENT 'Slug',
    description TEXT,
    unit_price DECIMAL(12,2) NOT NULL COMMENT 'Đơn giá sản phẩm',
    commission_rate DECIMAL(5,2) DEFAULT 0.00 COMMENT 'Tỷ lệ hoa hồng cho nhà cung cấp',
    discount_percentage DECIMAL(5,2) DEFAULT 0.00,
    energy_efficiency_rating INT CHECK (energy_efficiency_rating >= 0 AND energy_efficiency_rating <= 5) COMMENT 'Xếp hạng hiệu suất năng lượng (0-5)',
    specifications JSON COMMENT 'Thông số kỹ thuật dưới dạng cặp khóa-giá trị',
    manual_urls VARCHAR(1000) COMMENT 'URL hướng dẫn/sổ tay, phân cách bằng dấu phẩy',
    public_url VARCHAR(500) COMMENT 'URL công khai cho manual files',
    warranty_months INT DEFAULT 12,
    weight_kg DECIMAL(10,3) NOT NULL COMMENT 'Trọng lượng sản phẩm (kg) - bắt buộc',
    dimensions_cm JSON COMMENT '{chiều dài, chiều rộng, chiều cao}',
    registration_id BIGINT UNSIGNED NULL COMMENT 'Khóa ngoại đến product_registrations',
    snapshot_type ENUM('proposed', 'history') NOT NULL COMMENT 'Loại snapshot: proposed (đề xuất chờ duyệt), history (lịch sử đã được duyệt và áp dụng vào product)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    FOREIGN KEY (category_id) REFERENCES product_categories(id) ON DELETE RESTRICT,
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (registration_id) REFERENCES product_registrations(id) ON DELETE RESTRICT,
    
    INDEX idx_product_snapshot_created (product_id, snapshot_type, created_at) COMMENT 'Optimized cho history pagination',
    INDEX idx_vendor_snapshot (vendor_id, snapshot_type) COMMENT 'Cho queries filter theo vendor và snapshot type'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Lưu trữ toàn bộ lịch sử snapshot thay đổi của sản phẩm (cả đề xuất và đã duyệt)';

-- Bảng yêu cầu cập nhật sản phẩm (chỉ chứa thông tin quản lý)
CREATE TABLE product_update_requests (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_snapshot_id BIGINT UNSIGNED NOT NULL UNIQUE COMMENT 'ID snapshot đề xuất thay đổi - quan hệ 1:1 với product_snapshot',
    product_id BIGINT UNSIGNED NOT NULL COMMENT 'ID sản phẩm cần cập nhật',
    status ENUM('pending', 'approved', 'rejected') DEFAULT 'pending',
    rejection_reason VARCHAR(500) NULL COMMENT 'Lý do từ chối nếu Staff không duyệt',    
    processed_by BIGINT UNSIGNED NULL COMMENT 'Staff/Admin thực hiện duyệt',
    processed_at TIMESTAMP NULL,  
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (product_snapshot_id) REFERENCES product_snapshot(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    FOREIGN KEY (processed_by) REFERENCES users(id) ON DELETE RESTRICT,
    
    INDEX idx_product_status (product_id, status) COMMENT 'Cho queries check pending request của product',
    INDEX idx_status_updated (status, updated_at) COMMENT 'Optimized cho admin pagination theo status'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Quản lý yêu cầu cập nhật sản phẩm (chỉ chứa thông tin trạng thái và duyệt)';

-- Thông tin chứng chỉ của sản phẩm
CREATE TABLE product_certificates (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NULL,
    registration_id BIGINT UNSIGNED NULL COMMENT 'Khóa ngoại đến product_registrations',
    certification_code VARCHAR(50) NOT NULL,
    certification_name VARCHAR(255) NOT NULL,
    status ENUM('pending', 'verified', 'rejected') DEFAULT 'pending',
    rejection_reason VARCHAR(500) NULL COMMENT 'Lý do từ chối nếu trạng thái bị từ chối',
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    verified_at TIMESTAMP NULL,
    verified_by BIGINT UNSIGNED NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    FOREIGN KEY (registration_id) REFERENCES product_registrations(id) ON DELETE RESTRICT,
    FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_product_status (product_id, status) COMMENT 'Cho queries filter certificates theo product và status',
    INDEX idx_registration (registration_id),
    INDEX idx_status_uploaded (status, uploaded_at) COMMENT 'Optimized cho admin queue pagination'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Chứng chỉ bền vững do sản phẩm gắn kết để xác minh';

-- Đánh giá và xếp hạng
CREATE TABLE product_reviews (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    order_id BIGINT UNSIGNED NOT NULL,
    customer_id BIGINT UNSIGNED NOT NULL,
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    UNIQUE KEY unique_product_order_customer (product_id, order_id, customer_id),
    INDEX idx_product_created (product_id, created_at) COMMENT 'Optimized cho pagination reviews theo product',
    INDEX idx_order_created (order_id, created_at) COMMENT 'Cho pagination reviews theo order',
    INDEX idx_customer_created (customer_id, created_at) COMMENT 'Cho pagination reviews theo customer'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Đánh giá và xếp hạng sản phẩm';

-- Bảng quản lý media tập trung
CREATE TABLE media_links (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    owner_type ENUM('vendor_certificates', 'chatbot_messages', 'products', 'product_registrations', 'product_certificates', 'product_reviews', 'forum_posts', 'request_message', 'product_snapshot', 'customer_vendor_messages') NOT NULL,
    owner_id BIGINT UNSIGNED NOT NULL,
    image_url VARCHAR(1024) NOT NULL COMMENT 'URL hình ảnh trên cloud storage',
    image_public_id VARCHAR(512) NULL COMMENT 'Public ID từ cloud storage (Cloudinary, S3, etc.)',
    purpose ENUM('front', 'back', 'none', 'vendorcertificate_pdf','productcertificate_pdf','productimage') DEFAULT 'none' COMMENT 'Mục đích của hình ảnh: front (ảnh chính), back (ảnh phụ), none (không xác định)',
    sort_order INT NOT NULL DEFAULT 0 COMMENT 'Thứ tự hiển thị',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_owner_sort (owner_type, owner_id, sort_order) COMMENT 'Optimized cho queries ORDER BY sort_order'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng quản lý media tập trung cho tất cả các thực thể';

-- =========================
-- Cart, Orders & payments
-- =========================

-- Bảng giỏ hàng
CREATE TABLE cart (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    customer_id BIGINT UNSIGNED NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_customer (customer_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Giỏ hàng cho người dùng';

CREATE TABLE cart_items (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    cart_id BIGINT UNSIGNED NOT NULL,
    product_id BIGINT UNSIGNED NOT NULL,
    quantity INT NOT NULL DEFAULT 1 CHECK (quantity > 0),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (cart_id) REFERENCES cart(id) ON DELETE RESTRICT,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    UNIQUE KEY unique_cart_product (cart_id, product_id),
    INDEX idx_cart (cart_id),
    INDEX idx_product (product_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cart items for customer shopping carts';

-- Bảng đơn hàng
CREATE TABLE orders (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    customer_id BIGINT UNSIGNED NOT NULL,
    status ENUM('pending', 'processing', 'paid', 'shipped', 'delivered', 'cancelled', 'refunded', 'partial-refund') DEFAULT 'pending',
    subtotal DECIMAL(12,2) NOT NULL,
    tax_amount DECIMAL(12,2) DEFAULT 0.00,
    shipping_fee DECIMAL(12,2) DEFAULT 0.00,
    discount_amount DECIMAL(12,2) DEFAULT 0.00,
    total_amount DECIMAL(12,2) NOT NULL,
    address_id BIGINT UNSIGNED NOT NULL,
    order_payment_method ENUM('Banking', 'COD') NOT NULL,
    shipping_method VARCHAR(100),
    tracking_number VARCHAR(100),
    notes VARCHAR(500),
    courier_id INT NOT NULL COMMENT 'Mã đơn vị vận chuyển/nhà cung cấp vận chuyển',
    width INT NOT NULL COMMENT 'Chiều rộng gói hàng (cm)',
    height INT NOT NULL COMMENT 'Chiều cao gói hàng (cm)',
    length INT NOT NULL COMMENT 'Chiều dài gói hàng (cm)',
    weight INT NOT NULL COMMENT 'Cân nặng gói hàng (gram)',
    is_wallet_credited BOOLEAN DEFAULT FALSE COMMENT 'Đã cộng tiền vào ví vendor chưa',
    
    cancelled_reason VARCHAR(500),
    cancelled_at TIMESTAMP NULL,
    confirmed_at TIMESTAMP NULL,
    shipped_at TIMESTAMP NULL,
    delivered_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (address_id) REFERENCES addresses(id) ON DELETE RESTRICT,
    INDEX idx_customer_created (customer_id, created_at) COMMENT 'Optimized cho customer order history pagination',
    INDEX idx_address (address_id),
    INDEX idx_status_created (status, created_at) COMMENT 'Optimized cho vendor revenue queries và admin filters',
    INDEX idx_wallet_status_delivered (is_wallet_credited, status, delivered_at) COMMENT 'Optimized cho wallet credit cron job'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Đơn hàng của khách hàng';

-- Chi tiết đơn hàng
CREATE TABLE order_details (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    order_id BIGINT UNSIGNED NOT NULL,
    product_id BIGINT UNSIGNED NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(12,2) NOT NULL,
    discount_amount DECIMAL(12,2) DEFAULT 0.00,
    subtotal DECIMAL(12,2) NOT NULL,
    is_refunded BOOLEAN DEFAULT FALSE COMMENT 'Sản phẩm đã được hoàn tiền chưa',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, 

    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    INDEX idx_order (order_id),
    INDEX idx_product (product_id) COMMENT 'Cho revenue analytics JOIN queries',
    INDEX idx_refunded (is_refunded) COMMENT 'Cho partial refund tracking'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Các mục trong đơn hàng';

-- =========================
-- INVENTORY
-- =========================

-- Theo dõi tồn kho nhập (hàng vào)
CREATE TABLE batch_inventory (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    sku VARCHAR(100) NOT NULL COMMENT 'Mã quản lý kho - mã nhận dạng duy nhất cho lô hàng này',
    vendor_id BIGINT UNSIGNED NULL COMMENT 'Nhà cung cấp đã cung cấp sản phẩm (nếu có)',
    batch_number VARCHAR(100) NOT NULL COMMENT 'Số lô hoặc số lô hàng để theo dõi',
    lot_number VARCHAR(100) NOT NULL COMMENT 'Số lô sản xuất',
    quantity INT NOT NULL COMMENT 'Số lượng nhập',
    unit_cost_price DECIMAL(12,2) NOT NULL COMMENT 'Giá vốn mỗi đơn vị khi mua',
    expiry_date DATE NULL COMMENT 'Ngày hết hạn sản phẩm nếu có',
    manufacturing_date DATE NULL COMMENT 'Ngày sản xuất nếu có',
    notes VARCHAR(500) NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_product_created (product_id, created_at) COMMENT 'Optimized cho pagination theo product',
    INDEX idx_sku (sku),
    INDEX idx_vendor_created (vendor_id, created_at) COMMENT 'Optimized cho pagination theo vendor',
    INDEX idx_lot_product (lot_number, product_id) COMMENT 'Cho queries filter theo lot_number + product',
    INDEX idx_batch_number (batch_number) COMMENT 'Cho batch number uniqueness check'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Theo dõi tồn kho nhập - hàng vào với thông tin nhận hàng chi tiết';

-- Quản lý số seri máy móc trong lô
CREATE TABLE product_serials (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    batch_inventory_id BIGINT UNSIGNED NOT NULL,
    product_id BIGINT UNSIGNED NOT NULL,
    serial_number VARCHAR(255) UNIQUE NOT NULL COMMENT 'Số seri sản phẩm (có thể chứa chữ và số)',
    status ENUM('stock', 'sold', 'refund', 'adjustment') DEFAULT 'stock' COMMENT 'Trạng thái sản phẩm: stock (trong kho), sold (đã bán), refund (đã hoàn trả), adjustment (điều chỉnh)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (batch_inventory_id) REFERENCES batch_inventory(id) ON DELETE RESTRICT,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    INDEX idx_batch (batch_inventory_id),
    INDEX idx_product_status (product_id, status) COMMENT 'Optimized cho queries tìm available stock serials',
    INDEX idx_product_created (product_id, created_at) COMMENT 'Cho pagination theo product'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Quản lý số seri từng sản phẩm trong lô hàng';

-- Theo dõi tồn kho bán hàng (hàng ra)
CREATE TABLE export_inventory (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    product_serial_id BIGINT UNSIGNED NULL COMMENT 'ID số seri sản phẩm được xuất (cho máy móc/thiết bị)',
    lot_number VARCHAR(100) NOT NULL COMMENT 'Số lô sản xuất cho sản phẩm không có serial (phân bón, vật tư)',
    order_detail_id BIGINT UNSIGNED NULL COMMENT 'Chi tiết đơn hàng gây ra xuất kho',
    quantity INT NOT NULL DEFAULT 1 COMMENT 'Số lượng xuất kho',
    refund_quantity INT NOT NULL DEFAULT 0 COMMENT 'Số lượng đã hoàn trả',
    movement_type ENUM('sale', 'return to vendor', 'damage', 'loss', 'adjustment') DEFAULT 'sale',
    notes VARCHAR(500) NULL,
    created_by BIGINT UNSIGNED NOT NULL COMMENT 'Người thực hiện xuất kho',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    FOREIGN KEY (product_serial_id) REFERENCES product_serials(id) ON DELETE RESTRICT,
    FOREIGN KEY (order_detail_id) REFERENCES order_details(id) ON DELETE RESTRICT,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT,
    UNIQUE KEY uk_order_detail_lot (order_detail_id, lot_number, product_serial_id),
    INDEX idx_product_lot (product_id, lot_number) COMMENT 'Cho queries tính remaining quantity theo lot',
    INDEX idx_order_detail (order_detail_id),
    INDEX idx_serial (product_serial_id),
    INDEX idx_product_created (product_id, created_at) COMMENT 'Cho pagination export history',
    INDEX idx_movement_created (movement_type, created_at) COMMENT 'Cho filter theo movement type'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Theo dõi xuất kho - hỗ trợ cả sản phẩm có serial (máy móc) và không có serial (phân bón)';

-- =====================================================
-- CÁC BẢNG QUẢN LÝ YÊU CẦU
-- =====================================================

-- Bảng yêu cầu tổng quát cho các loại yêu cầu khác nhau
CREATE TABLE requests (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    request_type ENUM('refund_request', 'support_request') NOT NULL,
    title VARCHAR(255) NOT NULL COMMENT 'Tiêu đề yêu cầu',
    status ENUM('pending', 'in_review', 'approved', 'rejected', 'completed', 'cancelled') DEFAULT 'pending',
    processed_by BIGINT UNSIGNED NULL COMMENT 'Admin/nhân viên đã xử lý yêu cầu',
    processed_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (processed_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_user_updated (user_id, updated_at) COMMENT 'Optimized cho user request history pagination',
    INDEX idx_type_status_updated (request_type, status, updated_at) COMMENT 'Optimized cho admin queues filter + sort'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Hệ thống quản lý yêu cầu tổng quát';

-- Bảng tin nhắn yêu cầu (lưu trữ nội dung trao đổi giữa user và admin)
CREATE TABLE request_messages (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    request_id BIGINT UNSIGNED NOT NULL,
    staff_id BIGINT UNSIGNED NULL COMMENT 'Admin/staff phản hồi tin nhắn (NULL nếu là tin nhắn từ user)',
    description TEXT NOT NULL COMMENT 'Nội dung tin nhắn',
    reply_notes TEXT NULL COMMENT 'Ghi chú phản hồi từ admin/staff',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (request_id) REFERENCES requests(id) ON DELETE RESTRICT,
    FOREIGN KEY (staff_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_request (request_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Tin nhắn trao đổi trong yêu cầu hỗ trợ/hoàn tiền';

-- =====================================================
-- CÁC BẢNG ĐƠN HÀNG VÀ THANH TOÁN
-- =====================================================

-- Bảng thanh toán (chỉ lưu thông tin đặc thù về payment gateway)
CREATE TABLE payments (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    transaction_id BIGINT UNSIGNED NOT NULL COMMENT 'Tham chiếu đến bảng transactions (nguồn sự thật cho amount, status, order_id, timestamps)',
    payment_method ENUM('credit_card', 'debit_card', 'stripe', 'cod', 'payos') NOT NULL,
    payment_gateway ENUM('stripe', 'manual', 'payos') NOT NULL,
    gateway_response JSON COMMENT 'Phản hồi thô từ cổng thanh toán',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE RESTRICT,
    INDEX idx_transaction (transaction_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Thông tin đặc thù về payment gateway - amount, status, order_id, gateway_payment_id nằm trong transactions';


-- Bảng giao dịch (sổ cái trung tâm - nguồn sự thật duy nhất cho tất cả các chuyển động tài chính)
CREATE TABLE transactions (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    transaction_type ENUM('payment_in', 'wallet_cashout', 'wallet_topup', 'refund', 'adjustment', 'vendor_subscription') NOT NULL,
    amount DECIMAL(12,2) NOT NULL COMMENT 'Số tiền giao dịch - NGUỒN SỰ THẬT DUY NHẤT',
    currency VARCHAR(3) DEFAULT 'VND',
    user_id BIGINT UNSIGNED NOT NULL COMMENT 'Người dùng liên quan đến giao dịch này (khách hàng hoặc nhà cung cấp)',
    order_id BIGINT UNSIGNED NULL COMMENT 'Đơn hàng liên quan (cho payment_in từ khách hàng)',
    bank_account_id BIGINT UNSIGNED NULL COMMENT 'Tài khoản ngân hàng sử dụng cho giao dịch này (người mua thanh toán hoặc người nhận rút tiền)',
    status ENUM('pending', 'completed','failed','cancelled') NOT NULL DEFAULT 'pending' COMMENT 'Trạng thái giao dịch - NGUỒN SỰ THẬT DUY NHẤT',
    note VARCHAR(255) NOT NULL COMMENT 'Mô tả có thể đọc được',
    gateway_payment_id VARCHAR(255) NULL COMMENT 'ID giao dịch từ cổng thanh toán (nếu có)',    
    created_by BIGINT UNSIGNED NULL COMMENT 'Người dùng khởi tạo giao dịch này',
    processed_by BIGINT UNSIGNED NULL COMMENT 'Người dùng xử lý giao dịch này',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMP NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    FOREIGN KEY (bank_account_id) REFERENCES user_bank_accounts(id) ON DELETE RESTRICT,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (processed_by) REFERENCES users(id) ON DELETE RESTRICT,
    
    INDEX idx_order (order_id),
    INDEX idx_gateway_payment (gateway_payment_id),
    INDEX idx_user_type_status_created (user_id, transaction_type, status, created_at) COMMENT 'Composite index cho pagination queries by user',
    INDEX idx_type_status_created (transaction_type, status, created_at) COMMENT 'Composite index cho admin pagination queries'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Sổ cái tài chính trung tâm - lưu amount, status, bank_account_id, timestamps (single source of truth)';

-- Bảng rút tiền (chỉ lưu thông tin đặc thù về cashout)
CREATE TABLE cashouts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    transaction_id BIGINT UNSIGNED NOT NULL COMMENT 'Tham chiếu đến bảng transactions (nguồn sự thật cho amount, status, user_id, bank_account_id, timestamps)',
    reference_type ENUM('vendor_withdrawal', 'refund', 'admin_adjustment') NOT NULL COMMENT 'Loại tham chiếu: vendor_withdrawal (vendor rút tiền), refund (hoàn tiền cho khách), admin_adjustment (điều chỉnh bởi admin)',
    reference_id BIGINT UNSIGNED NULL COMMENT 'ID của thực thể tham chiếu (request_id cho refund, wallet_id cho vendor_withdrawal)',
    notes VARCHAR(500) NULL COMMENT 'Ghi chú bổ sung về cashout',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE RESTRICT,
    INDEX idx_transaction (transaction_id),
    INDEX idx_reference (reference_type, reference_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Thông tin đặc thù về rút tiền - amount, status, user_id, bank_account_id nằm trong transactions';

-- Ví cho nhà cung cấp (một ví cho một nhà cung cấp)
CREATE TABLE wallets (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL UNIQUE,
    balance DECIMAL(12,2) NOT NULL DEFAULT 0.00 COMMENT 'Số dư khả dụng',
    last_updated_by BIGINT UNSIGNED NULL COMMENT 'Người dùng thực hiện thay đổi số dư gần nhất',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (last_updated_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_vendor (vendor_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Ví nhà cung cấp theo dõi số dư';

-- =====================================================
-- CÁC BẢNG THÔNG BÁO
-- =====================================================

-- Bảng thông báo cho người dùng
CREATE TABLE notifications (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL COMMENT 'Người nhận thông báo (customer, vendor, staff, admin)',
    title VARCHAR(255) NOT NULL COMMENT 'Tiêu đề thông báo (hiển thị ngắn gọn)',
    message TEXT NOT NULL COMMENT 'Nội dung chi tiết thông báo',
    reference_type ENUM('order', 'payment', 'request', 'forum_post', 'chatbot_conversation', 'refund', 'wallet_cashout', 'product_registration', 'environmental_data', 'product_update_request') NULL COMMENT 'Loại entity tham chiếu (nếu có) - dùng để link đến chi tiết',
    reference_id BIGINT UNSIGNED NULL COMMENT 'ID của entity tham chiếu (ví dụ: order_id, post_id)',
    is_read BOOLEAN DEFAULT FALSE COMMENT 'Thông báo đã đọc chưa (dùng để filter hiển thị)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_user_read_created (user_id, is_read, created_at) COMMENT 'Optimized cho pagination với filter unread',
    INDEX idx_reference (reference_type, reference_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng lưu trữ thông báo cho người dùng, hỗ trợ real-time qua SignalR. Hard delete khi user xóa.';

-- =====================================================
-- THAY ĐỔI PHIÊN BẢN 10.1 (từ v10)
-- =====================================================

-- IX) BỔ SUNG TÍNH NĂNG QUẢN LÝ YÊU CẦU CẬP NHẬT SẢN PHẨM VÀ HOÀN TIỀN TỪNG PHẦN

-- A) BẢNG MỚI: product_snapshot
--   • Mục đích: Lưu trữ toàn bộ lịch sử snapshot thay đổi của sản phẩm (cả đề xuất và đã duyệt)
--   • Cấu trúc:
--     + product_id: Liên kết đến sản phẩm gốc
--     + snapshot_type: ENUM('proposed', 'history') - phân biệt loại snapshot
--       * proposed: Snapshot đề xuất thay đổi (chờ duyệt)
--       * history: Snapshot lịch sử đã được duyệt và áp dụng vào product
--     + Các trường sản phẩm: Snapshot đầy đủ thông tin sản phẩm (category_id, vendor_id, product_code, product_name, slug, description, unit_price, commission_rate, discount_percentage, energy_efficiency_rating, specifications, manual_urls, public_url, warranty_months, weight_kg, dimensions_cm, registration_id)
--   • Foreign Keys:
--     + product_id → products(id) ON DELETE CASCADE
--     + category_id → product_categories(id) ON DELETE RESTRICT
--     + vendor_id → users(id) ON DELETE CASCADE
--     + registration_id → product_registrations(id) ON DELETE RESTRICT
--   • Indexes:
--     + idx_product (product_id): Tối ưu query theo sản phẩm
--     + idx_vendor (vendor_id): Tối ưu query theo vendor
--     + idx_snapshot_type (snapshot_type): Tối ưu query theo loại snapshot
--     + idx_product_snapshot_type (product_id, snapshot_type): Composite index cho query lịch sử
--   • Lưu ý: Không có UNIQUE constraint cho product_code và slug vì đây là lịch sử snapshot

-- B) BẢNG MỚI: product_update_requests (refactored)
--   • Mục đích: Quản lý yêu cầu cập nhật sản phẩm (chỉ chứa thông tin trạng thái và duyệt)
--   • Cấu trúc:
--     + product_snapshot_id: FK đến product_snapshot (snapshot đề xuất)
--     + product_id: FK đến products (sản phẩm cần cập nhật)
--     + status: ENUM('pending', 'approved', 'rejected')
--     + rejection_reason: Lý do từ chối
--     + processed_by, processed_at: Thông tin xử lý
--   • Foreign Keys:
--     + product_snapshot_id → product_snapshot(id) ON DELETE CASCADE
--     + product_id → products(id) ON DELETE CASCADE
--     + processed_by → users(id) ON DELETE RESTRICT
--   • Lưu ý: vendor_id có thể lấy từ product_snapshot.vendor_id hoặc products.vendor_id

-- C) CẬP NHẬT BẢNG media_links
--   • Thêm giá trị 'product_snapshot' vào ENUM owner_type
--   • Mục đích: Cho phép upload ảnh cho snapshot sản phẩm

-- D) CẬP NHẬT BẢNG notifications
--   • Thêm giá trị 'product_update_request' vào ENUM reference_type
--   • Mục đích: Gửi thông báo khi yêu cầu cập nhật sản phẩm được tạo/duyệt/từ chối

-- D) CẬP NHẬT BẢNG orders
--   • Thêm giá trị 'partial-refund' vào ENUM status
--   • Mục đích: Hỗ trợ trạng thái đơn hàng bị hoàn tiền một phần (một số sản phẩm được hoàn, không phải toàn bộ đơn)

-- E) CẬP NHẬT BẢNG order_details
--   • Thêm cột: is_refunded BOOLEAN DEFAULT FALSE
--   • Mục đích: Đánh dấu từng sản phẩm trong đơn hàng đã được hoàn tiền chưa (hỗ trợ partial refund)
--   • Comment: 'Sản phẩm đã được hoàn tiền chưa'

-- F) WORKFLOW YÊU CẦU CẬP NHẬT SẢN PHẨM (REFACTORED):
--   1. Vendor tạo yêu cầu:
--      a. Tạo product_snapshot mới với snapshot_type='proposed' (chứa thông tin sản phẩm mới)
--      b. Tạo product_update_requests với FK đến product_snapshot vừa tạo, status='pending'
--      c. Upload ảnh mới (nếu có) vào media_links với owner_type='product_snapshot', owner_id=product_snapshot.id
--      d. Gửi notification cho Staff/Admin với reference_type='product_update_request'
--   
--   2. Staff/Admin duyệt:
--      a. Nếu approve:
--         - Tạo product_snapshot mới với snapshot_type='history' (sao chép thông tin hiện tại từ products - lưu lại lịch sử cũ)
--         - Update bảng products với thông tin từ product_snapshot (snapshot_type='proposed')
--         - Copy ảnh từ media_links (owner_type='product_snapshot', owner_id=proposed_snapshot_id) sang (owner_type='products')
--         - Update product_snapshot (proposed).snapshot_type='history'
--         - Update product_update_requests.status='approved', processed_by, processed_at
--         - Gửi notification cho vendor với reference_type='product_update_request'
--      b. Nếu reject:
--         - Update product_update_requests.status='rejected', rejection_reason, processed_by, processed_at
--         - product_snapshot (proposed) vẫn giữ nguyên với snapshot_type='proposed' (để lưu lại lịch sử bị từ chối)
--         - Gửi notification cho vendor với reference_type='product_update_request'
--   
--   3. Partial Refund Workflow:
--      a. Admin đánh dấu order_details.is_refunded=TRUE cho các sản phẩm cần hoàn
--      b. Tính tổng số tiền hoàn = SUM(subtotal) WHERE is_refunded=TRUE
--      c. Tạo transaction (type='refund', amount=refund_amount)
--      d. Tạo cashout (reference_type='refund')
--      e. Update orders.status='partial-refund' (nếu chưa phải 'refunded' toàn bộ)

-- G) LỢI ÍCH CỦA KIẾN TRÚC MỚI (SNAPSHOT-BASED):
--   1. Quản lý yêu cầu cập nhật sản phẩm:
--      - Vendor có thể tự đề xuất cập nhật thông tin sản phẩm
--      - Staff kiểm soát chất lượng thông tin trước khi publish
--      - Lưu lại toàn bộ lịch sử thay đổi (audit trail) với product_snapshot
--      - Có thể rollback về phiên bản cũ nếu cần (từ snapshot history)
--      - Tách biệt rõ ràng giữa dữ liệu snapshot (product_snapshot) và quản lý workflow (product_update_requests)
--      - Dễ dàng so sánh sự khác biệt giữa các phiên bản sản phẩm
--   
--   2. Hoàn tiền linh hoạt:
--      - Hỗ trợ hoàn tiền từng phần (partial refund) thay vì chỉ toàn bộ đơn
--      - Dễ dàng tracking sản phẩm nào đã được hoàn
--      - Tính toán chính xác số tiền hoàn cho từng case

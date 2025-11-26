-- Lược đồ Cơ sở Dữ liệu VerdantTech Solutions
-- Nền tảng Thiết bị Nông nghiệp Xanh Tích hợp AI cho Trồng Rau Bền vững
-- Phiên bản: 10
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
    INDEX idx_role (role),
    INDEX idx_status (status),
    INDEX idx_created_at (created_at)
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
    INDEX idx_user_id (user_id),
    INDEX idx_address_id (address_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng trung gian quản lý nhiều địa chỉ cho người dùng';

-- Hồ sơ nhà cung cấp cho người bán
CREATE TABLE vendor_profiles (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL UNIQUE,
    company_name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    business_registration_number VARCHAR(100) UNIQUE,
    notes VARCHAR(255) NULL,
    verified_at TIMESTAMP NULL,
    verified_by BIGINT UNSIGNED NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_company_name (company_name)
    -- Note: idx_verified (verified_at) đã bị xóa vì query sử dụng user_id làm filter chính (đã có UNIQUE constraint)
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
    INDEX idx_vendor (vendor_id),
    INDEX idx_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Chứng chỉ bền vững do nhà cung cấp tải lên để xác minh';

-- Tài khoản ngân hàng của người dùng (user và vendor đều có thể có nhiều tài khoản ngân hàng)
CREATE TABLE user_bank_accounts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    bank_code VARCHAR(20) NOT NULL,
    account_number VARCHAR(50) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    UNIQUE KEY unique_user_bank_account (user_id, account_number),
    INDEX idx_user (user_id),
    INDEX idx_is_active (is_active)
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
    INDEX idx_user (user_id),
    INDEX idx_address (address_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Chi tiết hồ sơ trang trại cho người dùng nông dân';

-- Bảng cây trồng cho trang trại
CREATE TABLE crops (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    farm_profile_id BIGINT UNSIGNED NOT NULL,
    crop_name VARCHAR(255) NOT NULL COMMENT 'Tên loại cây trồng',
    planting_date DATE NOT NULL COMMENT 'Ngày trồng',
    is_active BOOLEAN DEFAULT TRUE COMMENT 'Trạng thái hoạt động',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (farm_profile_id) REFERENCES farm_profiles(id) ON DELETE RESTRICT,
    INDEX idx_farm_profile (farm_profile_id),
    INDEX idx_is_active (is_active)
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
    INDEX idx_customer (customer_id),
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
    INDEX idx_conversation (conversation_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Các tin nhắn chatbot riêng lẻ';

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
    INDEX idx_category (forum_category_id),
    INDEX idx_user (user_id),
    INDEX idx_slug (slug),
    INDEX idx_status (status),
    INDEX idx_created (created_at)
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
    INDEX idx_post (forum_post_id),
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
    INDEX idx_slug (slug)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Danh mục sản phẩm theo cấp bậc';

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
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (category_id) REFERENCES product_categories(id) ON DELETE RESTRICT,
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_category (category_id),
    INDEX idx_vendor (vendor_id),
    INDEX idx_product_code (product_code),
    INDEX idx_slug (slug),
    FULLTEXT idx_search (product_name, description)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Sản phẩm thiết bị nông nghiệp xanh trong kho công ty';

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
    INDEX idx_vendor (vendor_id),
    INDEX idx_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Product registrations';

-- Thông tin chứng chỉ của sản phẩm
CREATE TABLE product_certificates (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
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
    FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_product (product_id),
    INDEX idx_status (status)
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
    INDEX idx_product (product_id),
    INDEX idx_customer (customer_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Đánh giá và xếp hạng sản phẩm';

-- Bảng quản lý media tập trung
CREATE TABLE media_links (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    owner_type ENUM('vendor_certificates', 'chatbot_messages', 'products', 'product_registrations', 'product_certificates', 'product_reviews', 'forum_posts', 'request_message') NOT NULL,
    owner_id BIGINT UNSIGNED NOT NULL,
    image_url VARCHAR(1024) NOT NULL COMMENT 'URL hình ảnh trên cloud storage',
    image_public_id VARCHAR(512) NULL COMMENT 'Public ID từ cloud storage (Cloudinary, S3, etc.)',
    purpose ENUM('front', 'back', 'none', 'vendorcertificate_pdf','productcertificate_pdf','productimage') DEFAULT 'none' COMMENT 'Mục đích của hình ảnh: front (ảnh chính), back (ảnh phụ), none (không xác định)',
    sort_order INT NOT NULL DEFAULT 0 COMMENT 'Thứ tự hiển thị',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_owner (owner_type, owner_id),
    INDEX idx_owner_type (owner_type),
    INDEX idx_owner_id (owner_id)
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
    status ENUM('pending', 'processing', 'paid', 'shipped', 'delivered', 'cancelled', 'refunded') DEFAULT 'pending',
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
    
    cancelled_reason VARCHAR(500),
    cancelled_at TIMESTAMP NULL,
    confirmed_at TIMESTAMP NULL,
    shipped_at TIMESTAMP NULL,
    delivered_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (address_id) REFERENCES addresses(id) ON DELETE RESTRICT,
    INDEX idx_customer (customer_id),
    INDEX idx_address (address_id),
    INDEX idx_status (status),
    INDEX idx_created (created_at)
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
    is_wallet_credited BOOLEAN DEFAULT FALSE COMMENT 'Đã cộng tiền vào ví vendor chưa',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, 

    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    INDEX idx_order (order_id),
    INDEX idx_wallet_credited_updated (is_wallet_credited, updated_at)
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
    quality_check_status ENUM('pending', 'passed', 'failed', 'not_required') DEFAULT 'not_required' COMMENT 'Trạng thái kiểm tra chất lượng',
    quality_checked_by BIGINT UNSIGNED NULL COMMENT 'Người thực hiện kiểm tra chất lượng',
    quality_checked_at TIMESTAMP NULL COMMENT 'Khi nào kiểm tra chất lượng được thực hiện',
    notes VARCHAR(500) NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (quality_checked_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_product (product_id),
    INDEX idx_sku (sku),
    INDEX idx_vendor (vendor_id),
    INDEX idx_quality_status (quality_check_status),
    INDEX idx_created (created_at)
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
    INDEX idx_product (product_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Quản lý số seri từng sản phẩm trong lô hàng';

-- Theo dõi tồn kho bán hàng (hàng ra)
CREATE TABLE export_inventory (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    product_serial_id BIGINT UNSIGNED NULL COMMENT 'ID số seri sản phẩm được xuất (cho máy móc/thiết bị)',
    lot_number VARCHAR(100) NULL COMMENT 'Số lô sản xuất cho sản phẩm không có serial (phân bón, vật tư)',
    order_id BIGINT UNSIGNED NULL COMMENT 'Đơn hàng gây ra xuất kho',
    movement_type ENUM('sale', 'return to vendor', 'damage', 'loss', 'adjustment') DEFAULT 'sale',
    notes VARCHAR(500) NULL,
    created_by BIGINT UNSIGNED NOT NULL COMMENT 'Người thực hiện xuất kho',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    FOREIGN KEY (product_serial_id) REFERENCES product_serials(id) ON DELETE RESTRICT,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_product (product_id),
    INDEX idx_order (order_id),
    INDEX idx_serial (product_serial_id),
    INDEX idx_lot (lot_number),
    INDEX idx_product_lot (product_id, lot_number)
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
    INDEX idx_user (user_id),
    INDEX idx_status (status)
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
    transaction_type ENUM('payment_in', 'wallet_cashout', 'refund', 'adjustment') NOT NULL,
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
    reference_type ENUM('order', 'payment', 'request', 'forum_post', 'chatbot_conversation', 'refund', 'wallet_cashout', 'product_registration', 'environmental_data') NULL COMMENT 'Loại entity tham chiếu (nếu có) - dùng để link đến chi tiết',
    reference_id BIGINT UNSIGNED NULL COMMENT 'ID của entity tham chiếu (ví dụ: order_id, post_id)',
    is_read BOOLEAN DEFAULT FALSE COMMENT 'Thông báo đã đọc chưa (dùng để filter hiển thị)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_user_read (user_id, is_read),
    INDEX idx_reference (reference_type, reference_id),
    INDEX idx_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng lưu trữ thông báo cho người dùng, hỗ trợ real-time qua SignalR. Hard delete khi user xóa.';

-- =====================================================
-- TỔNG QUAN THAY ĐỔI v10 (từ v9.2)
-- =====================================================

-- VIII) TÁI CẤU TRÚC PAYMENTS, TRANSACTIONS, CASHOUTS - SINGLE SOURCE OF TRUTH
-- • Mục đích: Đảm bảo transactions là nguồn sự thật duy nhất cho amount, status, timestamps
-- • Loại bỏ data redundancy và tăng tính toàn vẹn dữ liệu
-- • Quan hệ: Transaction 1:1 Payment, Transaction 1:1 Cashout (mỗi transaction chỉ có 1 payment HOẶC 1 cashout)

-- A) BẢNG TRANSACTIONS (Nguồn sự thật trung tâm):
--   • Thêm trường mới:
--     + order_id BIGINT UNSIGNED NULL: Liên kết trực tiếp với đơn hàng (cho payment_in)
--     + bank_account_id BIGINT UNSIGNED NULL: Tài khoản ngân hàng sử dụng (người mua thanh toán hoặc người nhận rút tiền)
--     + status thêm 'pending': Hỗ trợ trạng thái chờ xử lý
--   • Cập nhật comment:
--     + amount: 'NGUỒN SỰ THẬT DUY NHẤT' - rõ ràng đây là nơi lưu số tiền chính thống
--     + status: 'NGUỒN SỰ THẬT DUY NHẤT' - trạng thái giao dịch duy nhất
--   • Foreign Keys:
--     + Thêm FK order_id → orders(id)
--     + Thêm FK bank_account_id → user_bank_accounts(id)
--   • Indexes:
--     + Thêm idx_order (order_id): Tối ưu query theo đơn hàng
--     + Thêm idx_bank_account (bank_account_id): Tối ưu query theo tài khoản

-- B) BẢNG PAYMENTS (Chỉ lưu thông tin đặc thù về payment gateway):
--   • XÓA các trường:
--     + amount: Di chuyển về transactions (single source of truth)
--     + status: Di chuyển về transactions (tránh inconsistency)
--     + order_id: Di chuyển về transactions (single source of truth, tránh duplicate)
--     + gateway_payment_id: Di chuyển về transactions (để query dễ hơn, tránh duplicate)
--   • THÊM trường:
--     + transaction_id BIGINT UNSIGNED NOT NULL: FK bắt buộc đến transactions
--   • GIỮ LẠI các trường đặc thù:
--     + payment_method, payment_gateway: Phương thức thanh toán cụ thể
--     + gateway_response: Raw response từ gateway (audit trail)
--   • Foreign Keys:
--     + Thêm FK transaction_id → transactions(id) ON DELETE RESTRICT
--     + XÓA FK order_id (đã chuyển về transactions)
--   • Indexes:
--     + Thêm idx_transaction (transaction_id): Tối ưu join với transactions
--     + XÓA idx_status (không còn field status)
--     + XÓA idx_order (order_id đã chuyển về transactions)
--     + XÓA idx_gateway_payment (gateway_payment_id đã chuyển về transactions)
--   • Comment cập nhật: 'Thông tin đặc thù về payment gateway - amount, status, order_id, gateway_payment_id nằm trong transactions'

-- C) BẢNG CASHOUTS (Chỉ lưu thông tin đặc thù về cashout):
--   • XÓA các trường:
--     + user_id: Đã có trong transactions (tránh duplicate)
--     + amount: Di chuyển về transactions (single source of truth)
--     + status: Di chuyển về transactions (tránh inconsistency)
--     + bank_account_id: Di chuyển về transactions (dữ liệu chung, cả payment và cashout đều cần)
--   • CẬP NHẬT trường:
--     + transaction_id: NULL → NOT NULL (bắt buộc phải có transaction)
--     + reference_type: NULL → NOT NULL (luôn phải xác định loại cashout)
--   • GIỮ LẠI các trường đặc thù:
--     + reference_type, reference_id: Tham chiếu đến entity gốc (request, wallet)
--     + notes: Ghi chú bổ sung
--     + processed_by, processed_at: Thông tin xử lý bởi admin
--   • Foreign Keys:
--     + transaction_id: Vẫn giữ FK → transactions(id)
--     + XÓA FK user_id (không còn field này)
--     + XÓA FK bank_account_id (đã chuyển về transactions)
--   • Indexes:
--     + XÓA idx_user (không còn field user_id)
--     + XÓA idx_status (không còn field status)
--     + XÓA idx_bank_account (bank_account_id đã chuyển về transactions)
--     + Thêm idx_processed (processed_by, processed_at): Tối ưu query theo admin xử lý
--   • Comment cập nhật: 'Thông tin đặc thù về rút tiền - amount, status, user_id, bank_account_id nằm trong transactions'

-- D) LỢI ÍCH CỦA KIẾN TRÚC MỚI:
--   1. Data Integrity:
--      - Mỗi payment/cashout BẮT BUỘC phải có transaction tương ứng (quan hệ 1:1)
--      - Mỗi transaction chỉ có thể có 1 payment HOẶC 1 cashout, không thể có cả 2
--      - Không thể tạo payment/cashout mồ côi (orphan records)
--      - Amount và status luôn đồng bộ (chỉ có 1 nguồn duy nhất)
--   
--   2. Consistency:
--      - Không có tình trạng payment.amount ≠ transaction.amount
--      - Không có tình trạng payment.status ≠ transaction.status
--      - Dễ dàng rollback/update thông qua transaction record
--   
--   3. Audit Trail:
--      - Mọi chuyển động tiền đều được ghi lại trong transactions
--      - Dễ dàng theo dõi lịch sử tài chính của user
--      - Dễ dàng tạo báo cáo tổng hợp từ bảng transactions
--   
--   4. Simplified Queries:
--      - Query amount/status: Chỉ cần JOIN với transactions (1 nguồn duy nhất)
--      - Không cần UNION hoặc aggregate từ nhiều bảng
--      - Index hiệu quả hơn (không duplicate index giữa các bảng)
--   
--   5. Maintainability:
--      - Rõ ràng vai trò của từng bảng: transactions (chung), payments/cashouts (đặc thù)
--      - Dễ extend thêm payment method mới (chỉ thêm vào payments)
--      - Dễ extend thêm cashout type mới (chỉ thêm vào cashouts)

-- E) WORKFLOW THỰC TẾ:
--   1. Payment từ khách hàng:
--      a. Tạo transaction: (type='payment_in', amount=100000, status='pending', user_id=customer_id, order_id=123, bank_account_id=10, gateway_payment_id='ABC123')
--      b. Tạo payment: (transaction_id=1, payment_method='payos', payment_gateway='payos')
--      c. Khi gateway confirm: Update transaction.status='completed', transaction.completed_at=NOW()
--   
--   2. Cashout cho vendor:
--      a. Tạo transaction: (type='wallet_cashout', amount=50000, status='pending', user_id=vendor_id, bank_account_id=5)
--      b. Tạo cashout: (transaction_id=2, reference_type='vendor_withdrawal', reference_id=wallet_id)
--      c. Khi admin approve: Update transaction.status='completed', cashout.processed_by=admin_id
--   
--   3. Refund cho khách:
--      a. Tạo transaction: (type='refund', amount=20000, status='pending', user_id=customer_id, bank_account_id=8)
--      b. Tạo cashout: (transaction_id=3, reference_type='refund', reference_id=request_id)
--      c. Khi chuyển khoản xong: Update transaction.status='completed'

-- F) MIGRATION NOTES (Nếu có dữ liệu cũ):
--   • Bước 1: Tạo transaction cho mỗi payment hiện có (amount, status từ payment)
--   • Bước 2: Update payment.transaction_id = transaction.id tương ứng
--   • Bước 3: Tạo transaction cho mỗi cashout hiện có (amount, status, user_id từ cashout)
--   • Bước 4: Update cashout.transaction_id = transaction.id tương ứng
--   • Bước 5: Verify data consistency (payment count = transaction payment_in count, etc.)
--   • Bước 6: Drop columns: payment.amount, payment.status, cashout.user_id, cashout.amount, cashout.status
--   • Bước 7: Alter table: transaction_id NULL → NOT NULL cho cả payments và cashouts

-- Lược đồ Cơ sở Dữ liệu VerdantTech Solutions
-- Nền tảng Thiết bị Nông nghiệp Xanh Tích hợp AI cho Trồng Rau Bền vững
-- Phiên bản: 7.1
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
    province VARCHAR(100),
    district VARCHAR(100),
    commune VARCHAR(100),
    latitude DECIMAL(10,8) NULL COMMENT 'Vĩ độ',
    longitude DECIMAL(11,8) NULL COMMENT 'Kinh độ',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_province_district (province, district)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng địa chỉ chung cho các thực thể';

CREATE TABLE users (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role ENUM('customer', 'staff', 'vendor', 'admin') NOT NULL DEFAULT 'customer',
    full_name VARCHAR(255) NOT NULL,
    phone_number VARCHAR(20),
    tax_code VARCHAR(100) UNIQUE,
    address_id BIGINT UNSIGNED NULL,
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
    
    FOREIGN KEY (address_id) REFERENCES addresses(id) ON DELETE RESTRICT,
    INDEX idx_email (email),
    INDEX idx_role (role),
    INDEX idx_status (status),
    INDEX idx_address (address_id),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng xác thực và hồ sơ người dùng cơ bản';

-- Hồ sơ nhà cung cấp cho người bán
CREATE TABLE vendor_profiles (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL UNIQUE,
    company_name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    business_registration_number VARCHAR(100) UNIQUE,
    verified_at TIMESTAMP NULL,
    verified_by BIGINT UNSIGNED NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_company_name (company_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Hồ sơ nhà cung cấp/người bán và chi tiết xác minh';

-- Thông tin chứng chỉ của nhà cung cấp
CREATE TABLE vendor_certificates (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL,
    certification_code VARCHAR(50) NOT NULL,
    certification_name VARCHAR(255) NOT NULL,
    certificate_url VARCHAR(500) NOT NULL COMMENT 'URL đến hình ảnh/tập tin chứng chỉ đã tải lên',
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

-- Tài khoản ngân hàng của nhà cung cấp (một nhà cung cấp có thể có nhiều tài khoản ngân hàng)
CREATE TABLE vendor_bank_accounts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL,
    bank_code VARCHAR(20) NOT NULL,
    account_number VARCHAR(50) NOT NULL,
    account_holder VARCHAR(255) NOT NULL,
    is_default BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE RESTRICT,
    UNIQUE KEY unique_vendor_bank_account (vendor_id, account_number),
    INDEX idx_vendor (vendor_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Tài khoản ngân hàng của các hồ sơ nhà cung cấp';

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
    primary_crops VARCHAR(500) COMMENT 'Các loại cây trồng chính, danh sách phân cách bằng dấu phẩy',
    status ENUM('Active', 'Maintenance', 'Deleted') DEFAULT 'Active',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (address_id) REFERENCES addresses(id) ON DELETE RESTRICT,
    INDEX idx_user (user_id),
    INDEX idx_address (address_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Chi tiết hồ sơ trang trại cho người dùng nông dân';

-- Dữ liệu giám sát môi trường
CREATE TABLE environmental_data (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    farm_profile_id BIGINT UNSIGNED NOT NULL,
    customer_id BIGINT UNSIGNED NOT NULL,
    measurement_date DATE NOT NULL,
    soil_ph DECIMAL(3,1) CHECK (soil_ph >= 0 AND soil_ph <= 14),
    co2_footprint DECIMAL(10,2) COMMENT 'Lượng khí thải CO2 tính bằng kg',
    soil_moisture_percentage DECIMAL(5,2),
    soil_type ENUM('DatPhuSa', 'DatDoBazan', 'DatFeralit', 'DatThit', 'DatSet', 'DatCat') NOT NULL,
    notes VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (farm_profile_id) REFERENCES farm_profiles(id) ON DELETE RESTRICT,
    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_farm_date (farm_profile_id, measurement_date),
    INDEX idx_customer (customer_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Dữ liệu môi trường do nông dân nhập thủ công';

-- Theo dõi việc sử dụng phân bón
CREATE TABLE fertilizers (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    environmental_data_id BIGINT UNSIGNED NOT NULL,
    organic_fertilizer DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Phân hữu cơ (kg)',
    npk_fertilizer DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Phân NPK tổng hợp (kg)',
    urea_fertilizer DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Phân urê (kg)', 
    phosphate_fertilizer DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Phân lân (kg)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (environmental_data_id) REFERENCES environmental_data(id) ON DELETE RESTRICT,
    INDEX idx_environmental_data (environmental_data_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Dữ liệu sử dụng phân bón để tính toán lượng khí thải CO2';

-- Theo dõi việc sử dụng năng lượng
CREATE TABLE energy_usage (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    environmental_data_id BIGINT UNSIGNED NOT NULL,
    electricity_kwh DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Điện tiêu thụ (kWh)',
    gasoline_liters DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Xăng sử dụng (lít)',
    diesel_liters DECIMAL(10,2) DEFAULT 0.00 COMMENT 'Dầu diesel sử dụng (lít)',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,  

    FOREIGN KEY (environmental_data_id) REFERENCES environmental_data(id) ON DELETE RESTRICT,
    INDEX idx_environmental_data (environmental_data_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Dữ liệu sử dụng năng lượng để tính toán lượng khí thải CO2';

-- =====================================================
-- CÁC BẢNG CHATBOT AI
-- =====================================================

-- Các cuộc hội thoại chatbot
CREATE TABLE chatbot_conversations (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    customer_id BIGINT UNSIGNED NOT NULL,
    session_id VARCHAR(255) NOT NULL,
    title VARCHAR(255),
    context TEXT COMMENT 'Bối cảnh cuộc hội thoại và metadata',
    is_active BOOLEAN DEFAULT TRUE,
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ended_at TIMESTAMP NULL,

    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_customer (customer_id),
    INDEX idx_session (session_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Các phiên hội thoại chatbot';

-- Tin nhắn chatbot
CREATE TABLE chatbot_messages (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    conversation_id BIGINT UNSIGNED NOT NULL,
    message_type ENUM('user', 'bot', 'system') NOT NULL,
    message_text TEXT NOT NULL,
    attachments VARCHAR(1000) COMMENT 'URL đính kèm hình ảnh hoặc tập tin, phân cách bằng dấu phẩy',
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
    content JSON NOT NULL COMMENT 'Các khối nội dung hỗn hợp: [{"order": 1, "type": "text", "content": "Hello world"}, {"order": 2, "type": "image", "content": "https://example.com/image.jpg"}]',
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
    icon_url VARCHAR(500),
    is_active BOOLEAN DEFAULT TRUE,
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
    energy_efficiency_rating VARCHAR(10),
    specifications JSON COMMENT 'Thông số kỹ thuật dưới dạng cặp khóa-giá trị',
    manual_urls VARCHAR(1000) COMMENT 'URL hướng dẫn/sổ tay, phân cách bằng dấu phẩy',
    images VARCHAR(1000) COMMENT 'URL hình ảnh, phân cách bằng dấu phẩy',
    warranty_months INT DEFAULT 12,
    stock_quantity INT DEFAULT 0,
    weight_kg DECIMAL(10,3),
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
    energy_efficiency_rating VARCHAR(10),
    specifications JSON COMMENT 'Thông số kỹ thuật dưới dạng cặp khóa-giá trị',
    manual_urls VARCHAR(1000) COMMENT 'URL hướng dẫn/sổ tay, phân cách bằng dấu phẩy',
    images VARCHAR(1000) COMMENT 'URL hình ảnh, phân cách bằng dấu phẩy',
    warranty_months INT DEFAULT 12,
    weight_kg DECIMAL(10,3),
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
    certificate_url VARCHAR(500) NULL COMMENT 'URL đến hình ảnh/tập tin chứng chỉ đã tải lên',
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

-- =========================
-- Cart, Orders & payments
-- =========================

-- Bảng giỏ hàng
CREATE TABLE cart (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    customer_id BIGINT UNSIGNED NOT NULL,
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
    unit_price DECIMAL(12,2) NOT NULL COMMENT 'đơn giá',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (cart_id) REFERENCES cart(id) ON DELETE RESTRICT,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    UNIQUE KEY unique_cart_product (cart_id, product_id),
    INDEX idx_cart (cart_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cart items for customer shopping carts';

-- Bảng đơn hàng
CREATE TABLE orders (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    customer_id BIGINT UNSIGNED NOT NULL,
    status ENUM('pending', 'confirmed', 'processing', 'shipped', 'delivered', 'cancelled', 'refunded') DEFAULT 'pending',
    subtotal DECIMAL(12,2) NOT NULL,
    tax_amount DECIMAL(12,2) DEFAULT 0.00,
    shipping_fee DECIMAL(12,2) DEFAULT 0.00,
    discount_amount DECIMAL(12,2) DEFAULT 0.00,
    total_amount DECIMAL(12,2) NOT NULL,
    address_id BIGINT UNSIGNED NOT NULL,
    shipping_method VARCHAR(100),
    tracking_number VARCHAR(100),
    notes VARCHAR(500),
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
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, 

    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    INDEX idx_order (order_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Các mục trong đơn hàng';

-- Bảng thanh toán
CREATE TABLE payments (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    order_id BIGINT UNSIGNED NOT NULL,
    payment_method ENUM('credit_card', 'debit_card', 'paypal', 'stripe', 'bank_transfer', 'cod') NOT NULL,
    payment_gateway ENUM('stripe', 'paypal', 'vnpay', 'momo', 'manual') NOT NULL,
    gateway_payment_id VARCHAR(255) UNIQUE COMMENT 'ID giao dịch từ cổng thanh toán',
    amount DECIMAL(12,2) NOT NULL,
    status ENUM('pending', 'processing', 'completed', 'failed', 'refunded', 'partially_refunded') DEFAULT 'pending',
    gateway_response JSON COMMENT 'Phản hồi thô từ cổng thanh toán',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    INDEX idx_order (order_id),
    INDEX idx_gateway_payment (gateway_payment_id),
    INDEX idx_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='phương thức và trạng thái thanh toán cho đơn hàng';

-- Đánh giá và xếp hạng
CREATE TABLE product_reviews (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    order_id BIGINT UNSIGNED NOT NULL,
    customer_id BIGINT UNSIGNED NOT NULL,
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    title VARCHAR(255),
    comment TEXT,
    images VARCHAR(1000) COMMENT 'URL hình ảnh đánh giá, phân cách bằng dấu phẩy',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    UNIQUE KEY unique_product_order_customer (product_id, order_id, customer_id),
    INDEX idx_product (product_id),
    INDEX idx_customer (customer_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Đánh giá và xếp hạng sản phẩm';

-- =========================
-- INVENTORY
-- =========================

-- Theo dõi tồn kho nhập (hàng vào)
CREATE TABLE batch_inventory (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    sku VARCHAR(100) NOT NULL COMMENT 'Mã quản lý kho - mã nhận dạng duy nhất cho lô hàng này',
    vendor_id BIGINT UNSIGNED NULL COMMENT 'Nhà cung cấp đã cung cấp sản phẩm (nếu có)',
    batch_number VARCHAR(100) NULL COMMENT 'Số lô hoặc số lô hàng để theo dõi',
    lot_number VARCHAR(100) NULL COMMENT 'Số lô sản xuất',
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

-- Theo dõi tồn kho bán hàng (hàng ra)
CREATE TABLE export_inventory (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    order_id BIGINT UNSIGNED NULL COMMENT 'Đơn hàng gây ra chuyển động tồn kho này',
    quantity INT NOT NULL COMMENT 'Số lượng xuất',
    balance_after INT NOT NULL COMMENT 'Số dư sau khi xuất',
    movement_type ENUM('sale', 'return to vendor', 'damage', 'loss', 'adjustment') DEFAULT 'sale',
    notes VARCHAR(500) NULL,
    created_by BIGINT UNSIGNED NOT NULL COMMENT 'Người nhập xuất kho',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_product (product_id),
    INDEX idx_order (order_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Theo dõi tồn kho xuất - hàng ra với thông tin chi tiết';

-- =====================================================
-- CÁC BẢNG QUẢN LÝ YÊU CẦU
-- =====================================================

-- Bảng yêu cầu tổng quát cho các loại yêu cầu khác nhau
CREATE TABLE requests (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    request_type ENUM('refund_request', 'payout_request', 'support_request', 'vendor_register', 'product_registration', 'product_certification') NOT NULL,
    title VARCHAR(255) NOT NULL COMMENT 'Tiêu đề/chủ đề yêu cầu',
    description TEXT NOT NULL COMMENT 'Mô tả chi tiết về yêu cầu',
    status ENUM('pending', 'in_review', 'approved', 'rejected', 'completed', 'cancelled') DEFAULT 'pending',
    reply_notes TEXT NULL,
    processed_by BIGINT UNSIGNED NULL COMMENT 'Admin/nhân viên đã xử lý yêu cầu',
    processed_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (processed_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_user (user_id),
    INDEX idx_type_status (request_type, status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Hệ thống quản lý yêu cầu tổng quát';

-- =====================================================
-- CÁC BẢNG ĐƠN HÀNG VÀ THANH TOÁN
-- =====================================================

-- Bảng giao dịch (sổ cái trung tâm - nguồn sự thật duy nhất cho tất cả các chuyển động tài chính)
CREATE TABLE transactions (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    transaction_type ENUM('payment_in', 'cashout', 'wallet_credit', 'wallet_debit', 'commission', 'refund', 'adjustment') NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'VND',
    order_id BIGINT UNSIGNED NULL COMMENT 'Tham chiếu đến bảng đơn hàng',
    user_id BIGINT UNSIGNED NOT NULL COMMENT 'Người dùng liên quan đến giao dịch này (khách hàng hoặc nhà cung cấp)',
    status ENUM('pending','completed','failed','cancelled') NOT NULL DEFAULT 'pending',
    note VARCHAR(255) NOT NULL COMMENT 'Mô tả có thể đọc được',
    gateway_payment_id VARCHAR(255) NULL COMMENT 'ID giao dịch từ cổng thanh toán',    
    created_by BIGINT UNSIGNED NULL COMMENT 'Người dùng khởi tạo giao dịch này',
    processed_by BIGINT UNSIGNED NULL COMMENT 'Người dùng xử lý giao dịch này',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (processed_by) REFERENCES users(id) ON DELETE RESTRICT,
    
    INDEX idx_user (user_id),
    INDEX idx_type_status (transaction_type, status),
    INDEX idx_order (order_id),
    INDEX idx_gateway_payment (gateway_payment_id),
    INDEX idx_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Sổ cái tài chính trung tâm - nguồn sự thật duy nhất cho tất cả chuyển động tiền tệ';

-- Ví cho nhà cung cấp (một ví cho một nhà cung cấp)
CREATE TABLE wallets (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL UNIQUE,
    balance DECIMAL(12,2) NOT NULL DEFAULT 0.00 COMMENT 'Số dư khả dụng',
    last_transaction_id BIGINT UNSIGNED NULL COMMENT 'Giao dịch gần nhất ảnh hưởng đến số dư',
    last_updated_by BIGINT UNSIGNED NULL COMMENT 'Người dùng thực hiện thay đổi số dư gần nhất',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (last_transaction_id) REFERENCES transactions(id) ON DELETE RESTRICT,
    FOREIGN KEY (last_updated_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_vendor (vendor_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Ví nhà cung cấp theo dõi số dư';

-- Bảng rút tiền (tiền ra - thanh toán cho nhà cung cấp, chi phí)
CREATE TABLE cashouts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL,
    transaction_id BIGINT UNSIGNED NULL COMMENT 'Tham chiếu đến bảng giao dịch để đảm bảo tính nhất quán',
    bank_account_id BIGINT UNSIGNED NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    status ENUM('pending','processing','completed','failed','cancelled') NOT NULL DEFAULT 'pending',
    reason VARCHAR(255) NULL COMMENT 'Lý do hoặc mục đích của khoản rút tiền (ví dụ: Thanh toán hoa hồng, Hoàn tiền)',
    gateway_transaction_id VARCHAR(255) NULL COMMENT 'ID giao dịch cổng thanh toán bên ngoài',
    reference_type VARCHAR(50) NULL COMMENT 'Loại tham chiếu (đơn hàng, yêu cầu, v.v.)',
    reference_id BIGINT UNSIGNED NULL COMMENT 'ID của thực thể tham chiếu',
    notes VARCHAR(500) NULL,
    processed_by BIGINT UNSIGNED NULL COMMENT 'Admin đã xử lý lần rút tiền này',
    processed_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE RESTRICT,
    FOREIGN KEY (bank_account_id) REFERENCES vendor_bank_accounts(id) ON DELETE RESTRICT,
    FOREIGN KEY (processed_by) REFERENCES users(id) ON DELETE RESTRICT,
    UNIQUE KEY idx_unique_gateway_transaction (gateway_transaction_id),
    INDEX idx_vendor (vendor_id),
    INDEX idx_status (status),
    INDEX idx_transaction (transaction_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='bảng rút tiền cho vendor';

-- VerdantTech DB — So sánh thay đổi v7 → v7.1
-- Ngày so sánh: 2025-09-19 (GMT+7)
-- Nguồn: verdanttech_schema_v7.sql & verdanttech_schema_v7.1.sql

-- I) TỔNG QUAN THAY ĐỔI
-- • Thêm bảng địa chỉ chung `addresses`; thay thế các cột địa chỉ rải rác bằng khoá ngoại `address_id` ở `users`, `farm_profiles`, `orders`.
-- • Chuẩn hoá vai trò người mua: đổi `user_id` → `customer_id` tại nhiều bảng (`orders`, `cart`, `chatbot_conversations`, `environmental_data`).
-- • 20 khoá ngoại chuyển từ `ON DELETE CASCADE` → `ON DELETE RESTRICT` để tránh xoá dây chuyền ngoài ý muốn.
-- • Chuẩn hoá thực thể Vendor: chuyển tham chiếu từ `vendor_profiles` sang `users` ở một loạt bảng (ví dụ: `products`, `product_registrations`, `vendor_bank_accounts`, `vendor_certificates`, `cashouts`, `wallets`).
-- • Sản phẩm: đổi `name`→`product_name`, `price`→`unit_price`; bỏ `cost_price`, `total_reviews`; cập nhật chỉ mục FULLTEXT trên (`product_name`,`description`).
-- • Đơn hàng/vận chuyển: thay `shipping_address` (JSON) bằng `address_id` FK; đồng nhất chỉ mục `idx_created`.
-- • Thanh toán/tài chính: đơn giản hoá `payments` (loại `transaction_id`, các trường refund/paid/failed tách khỏi), tái cấu trúc `transactions` (bỏ trường ví & metadata cũ, thêm `gateway_payment_id`, `note`, ràng buộc với `user_id`/`order_id`), bổ sung liên kết chéo ở `wallets`.
-- • Cộng đồng: chuẩn tên cột FK (`category_id`→`forum_category_id`, `post_id`→`forum_post_id`) và tinh gọn index; toàn bộ FK đổi sang RESTRICT.
-- • Tồn kho: thêm/đổi mô tả, cập nhật `movement_type` (thêm 'return to vendor'), thêm `updated_at` cho `export_inventory`.

-- II) BẢNG MỚI / BỊ LOẠI BỎ
-- • Bảng mới: `addresses` (lưu địa chỉ dùng chung).
-- • Bảng bị loại bỏ: Không có (v7.1 giữ toàn bộ bảng của v7, chỉ nâng cấp cấu trúc).

-- III) THAY ĐỔI CHI TIẾT THEO BẢNG

-- A. addresses  — MỚI
--    - Cột: id, location_address, province, district, commune, latitude, longitude, created_at, updated_at
--    - Index: INDEX idx_province_district (province, district)

-- B. Người dùng & Vendor
--    • users
--      - Thêm cột: address_id, tax_code
--      - Thêm FK: (address_id) → addresses(id) ON DELETE RESTRICT
--      - Thêm Index: INDEX idx_address (address_id)
--    • vendor_profiles
--      - Xoá cột: company_address, province, district, commune
--      - Thêm/Gỡ FK: (user_id) đổi ON DELETE CASCADE → RESTRICT
--      - Gỡ Index: INDEX idx_slug (slug)
--    • vendor_bank_accounts
--      - Thêm/Gỡ FK: (vendor_id) đổi tham chiếu vendor_profiles(id) → users(id) và đổi CASCADE → RESTRICT
--      - Gỡ Index: INDEX idx_vendor_default (vendor_id, is_default)
--    • vendor_certificates
--      - Thay đổi FK: (vendor_id) tham chiếu vendor_profiles(id) (CASCADE) → users(id) (RESTRICT)
--      - Giữ/Chuẩn hoá FK verified_by → users(id) ON DELETE RESTRICT
--    • wallets
--      - Thêm cột: last_transaction_id, last_updated_by
--      - Xoá cột: pending_withdraw
--      - Đổi FK vendor_id: vendor_profiles(id) (CASCADE) → users(id) (RESTRICT)
--      - Thêm FK: last_transaction_id → transactions(id) RESTRICT; last_updated_by → users(id) RESTRICT
--      - Thêm Index: INDEX idx_vendor (vendor_id)

-- C. Trang trại & Môi trường
--    • farm_profiles
--      - Thêm cột: address_id
--      - Xoá cột: location_address, province, district, commune, latitude, longitude
--      - Đổi FK user_id: CASCADE → RESTRICT
--      - Thêm FK: address_id → addresses(id) RESTRICT
--      - Thêm Index: idx_address, idx_user
--      - Gỡ Index: idx_location, idx_farm_size, idx_status
--    • environmental_data
--      - Đổi tên cột: 'user_id' → 'customer_id'
--      - Đổi FK: (farm_profile_id) CASCADE → RESTRICT; (customer_id) RESTRICT (thay cho user_id trước đây)
--      - Thêm Index: idx_customer (customer_id)
--      - Gỡ Index: idx_user, idx_date, idx_soil_type
--    • fertilizers
--      - Đổi FK: (environmental_data_id) CASCADE → RESTRICT
--    • energy_usage
--      - Đổi FK: (environmental_data_id) CASCADE → RESTRICT

-- D. Chatbot
--    • chatbot_conversations
--      - Đổi tên cột: 'user_id' → 'customer_id'
--      - Đổi FK: (customer_id) RESTRICT (thay cho CASCADE trước đây)
--      - Thêm Index: idx_customer (customer_id), idx_session (session_id)
--      - Gỡ Index: idx_user_session, idx_active, idx_started
--    • chatbot_messages
--      - Đổi FK: (conversation_id) CASCADE → RESTRICT
--      - Gỡ Index: idx_type, idx_created

-- E. Cộng đồng
--    • forum_categories
--      - Không thay đổi cột; không đổi FK; không đổi index
--    • forum_posts
--      - Đổi tên cột: 'category_id' → 'forum_category_id'
--      - Xoá cột: last_activity_at
--      - Đổi FK: forum_category_id → forum_categories(id) RESTRICT; user_id → users(id) RESTRICT
--      - Thêm Index: idx_category (forum_category_id), idx_status (status), idx_created (created_at)
--      - Gỡ Index: idx_category (category_id), idx_status_pinned, idx_last_activity
--    • forum_comments
--      - Đổi tên cột: 'post_id' → 'forum_post_id'
--      - Đổi FK: forum_post_id → forum_posts(id) RESTRICT; user_id → users(id) RESTRICT; gỡ FK self parent_id (CASCADE)
--      - Thêm Index: idx_post (forum_post_id)
--      - Gỡ Index: idx_parent, idx_post (post_id)

-- F. Sản phẩm & Đăng ký
--    • product_categories
--      - Đổi FK self-referencing: parent_id ON DELETE CASCADE → RESTRICT
--      - Gỡ Index: idx_active (is_active)
--    • products
--      - Đổi tên cột: 'name' → 'product_name', 'price' → 'unit_price'
--      - Xoá cột: cost_price, total_reviews
--      - Sửa cột: commission_rate (cập nhật comment)
--      - Đổi FK: vendor_id → users(id) RESTRICT (trước kia là vendor_profiles)
--      - Thêm Index: idx_vendor (vendor_id)
--      - Đổi FULLTEXT: (name, description) → (product_name, description)
--      - Gỡ Index: idx_price, idx_active
--    • product_certificates
--      - Đổi FK: product_id CASCADE → RESTRICT
--      - Gỡ Index: idx_uploaded (uploaded_at)
--    • product_registrations
--      - Đổi tên cột: 'product_code' → 'proposed_product_code', 'name' → 'proposed_product_name', 'price' → 'unit_price'
--      - Thêm cột: updated_at, approved_at
--      - Xoá cột: commission_rate, reviewed_at
--      - Đổi FK: vendor_id → users(id) RESTRICT (trước kia là vendor_profiles)
--      - Thêm/Gỡ Index: thêm idx_status (status), idx_vendor (vendor_id); gỡ idx_vendor_status, idx_created
--    • product_reviews
--      - Đổi FK: customer_id, product_id, order_id từ CASCADE → RESTRICT
--      - Thêm/Gỡ Index: thêm idx_product (product_id); gỡ idx_product_rating, idx_created_at

-- G. Giỏ hàng & Đơn hàng & Thanh toán
--    • cart
--      - Đổi tên cột: 'user_id' → 'customer_id'
--      - Đổi FK: (customer_id) RESTRICT (thay cho CASCADE)
--      - Đổi Index: idx_user → idx_customer
--    • cart_items
--      - Sửa cột: unit_price (cập nhật comment: “đơn giá”)
--      - Đổi FK: (cart_id), (product_id) từ CASCADE → RESTRICT
--      - Gỡ Index: idx_product, idx_created_at (giữ idx_cart)
--    • orders
--      - Đổi tên cột: 'user_id' → 'customer_id'
--      - Xoá cột: shipping_address (JSON)
--      - Thêm cột: address_id
--      - Thêm FK: (address_id) → addresses(id) RESTRICT; (customer_id) → users(id) RESTRICT
--      - Thêm/Gỡ Index: thêm idx_address, idx_customer, idx_created; gỡ idx_created_at
--    • order_details
--      - Đổi FK: (order_id) CASCADE → RESTRICT; (product_id) giữ RESTRICT
--      - Gỡ Index: idx_product (chỉ còn idx_order)
--    • payments
--      - Thêm cột: gateway_payment_id
--      - Xoá cột: transaction_id, refund_amount, refund_reason, refunded_at, paid_at, failed_at, gateway_transaction_id
--      - Gỡ FK: (transaction_id) → transactions(id)
--      - Thêm/Gỡ Index: thêm idx_gateway_payment; gỡ idx_transaction, idx_gateway_transaction, idx_payment_method

-- H. Tồn kho
--    • batch_inventory
--      - Đổi tên cột: 'vendor_profile_id' → 'vendor_id'
--      - Sửa cột: quantity (thêm comment)
--      - Đổi FK: product_id từ CASCADE → RESTRICT; vendor_id → users(id) RESTRICT (thay vì vendor_profiles)
--      - Thêm/Gỡ Index: thêm idx_vendor, idx_created; gỡ idx_vendor(vendor_profile_id), idx_expiry_date
--    • export_inventory
--      - Thêm cột: updated_at
--      - Xoá cột: unit_sale_price
--      - Sửa cột: movement_type từ ENUM('sale','return','damage','loss','adjustment') thành ENUM('sale','return to vendor','damage','loss','adjustment')
--      - Sửa cột: quantity, balance_after, created_by (cập nhật chú thích)
--      - Đổi FK: product_id từ CASCADE → RESTRICT; thêm created_by → users(id) RESTRICT
--      - Gỡ Index: idx_movement_type

-- I. Yêu cầu & Giao dịch
--    • requests
--      - Thêm cột: reply_notes
--      - Xoá cột: admin_notes, amount, rejection_reason
--      - Sửa cột: user_id (NULL → NOT NULL)
--      - Sửa ENUM request_type: thêm 'product_registration', 'product_certification'; giữ các loại khác
--      - Thêm FK: processed_by → users(id) RESTRICT; user_id → users(id) RESTRICT (thay vì CASCADE)
--      - Thêm/Gỡ Index: thêm idx_user; gỡ idx_created_at
--    • transactions
--      - Thêm cột: user_id, order_id, gateway_payment_id, note, created_by (giữ), status (giữ), …
--      - Xoá cột: customer_id, vendor_id, wallet_id, balance_before, balance_after, description, metadata, reference_type, reference_id
--      - Đổi/Gỡ FK: gỡ các FK tới vendor_profiles, wallets; thêm FK user_id → users(id) RESTRICT
--      - Thêm/Gỡ Index: thêm idx_user, idx_gateway_payment, idx_created; gỡ idx_customer, idx_vendor, idx_wallet, idx_completed_at, idx_reference, idx_amount, idx_created_at (đổi tên), …
--    • cashouts
--      - Thêm cột: bank_account_id, reason
--      - Xoá cột: bank_code, bank_account_number, bank_account_holder, cashout_type
--      - Đổi FK: vendor_id → users(id) RESTRICT (trước kia vendor_profiles); thêm bank_account_id → vendor_bank_accounts(id) RESTRICT
--      - Thêm/Gỡ Index: thêm idx_transaction; gỡ idx_type

-- IV) TỔNG HỢP CHÍNH SÁCH XOÁ (FK)
-- • Tổng số FK chuyển từ CASCADE → RESTRICT: 20 (chi tiết liệt kê trong từng bảng ở mục III).

-- V) GHI CHÚ TƯƠNG THÍCH
-- • Khi nâng cấp dữ liệu: cần tạo `addresses` và migrate các cột địa chỉ cũ sang `addresses`, cập nhật các FK `address_id`.
-- • Cập nhật mã dịch vụ sử dụng các cột đổi tên (ví dụ `products.name`→`products.product_name`, `orders.user_id`→`orders.customer_id`, ...).
-- • Kiểm tra logic xoá: với RESTRICT, cần xoá theo thứ tự an toàn hoặc chuyển sang trạng thái 'inactive' thay vì xoá cứng.
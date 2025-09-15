-- Lược đồ Cơ sở Dữ liệu VerdantTech Solutions
-- Nền tảng Thiết bị Nông nghiệp Xanh Tích hợp AI cho Trồng Rau Bền vững
-- Phiên bản: 7.0
-- Engine: InnoDB (hỗ trợ giao dịch)
-- Bộ ký tự: utf8mb4 (hỗ trợ đa ngôn ngữ)

-- Tạo cơ sở dữ liệu
CREATE DATABASE IF NOT EXISTS verdanttech_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE verdanttech_db;

-- =====================================================
-- CÁC BẢNG QUẢN LÝ NGƯỜI DÙNG
-- =====================================================

-- Bảng người dùng (xác thực cơ bản)
CREATE TABLE users (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role ENUM('customer', 'staff', 'vendor', 'admin') NOT NULL DEFAULT 'customer',
    full_name VARCHAR(255) NOT NULL,
    phone_number VARCHAR(20),
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

-- Hồ sơ nhà cung cấp cho người bán
CREATE TABLE vendor_profiles (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL UNIQUE,
    company_name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    business_registration_number VARCHAR(100) UNIQUE,
    company_address TEXT,
    province VARCHAR(100),
    district VARCHAR(100),
    commune VARCHAR(100),
    verified_at TIMESTAMP NULL,
    verified_by BIGINT UNSIGNED NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_company_name (company_name),
    INDEX idx_slug (slug)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Hồ sơ nhà cung cấp/người bán và chi tiết xác minh';

-- Thông tin chứng chỉ của nhà cung cấp
CREATE TABLE vendor_certificates (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL,
    certification_id BIGINT UNSIGNED NOT NULL,
    certification_name VARCHAR(255) NOT NULL,
    certificate_url VARCHAR(500) NOT NULL COMMENT 'URL đến hình ảnh/tập tin chứng chỉ đã tải lên',
    status ENUM('pending', 'verified', 'rejected') DEFAULT 'pending',
    rejection_reason VARCHAR(500) NULL COMMENT 'Lý do từ chối nếu trạng thái bị từ chối',
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    verified_at TIMESTAMP NULL,
    verified_by BIGINT UNSIGNED NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_vendor (vendor_id),
    INDEX idx_status (status),
    INDEX idx_uploaded (uploaded_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Chứng chỉ bền vững do nhà cung cấp tải lên để xác minh';

-- Thông tin chứng chỉ của sản phẩm
CREATE TABLE product_certificates (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    certification_id BIGINT UNSIGNED NOT NULL,
    certificate_url VARCHAR(500) NULL COMMENT 'URL đến hình ảnh/tập tin chứng chỉ đã tải lên',
    status ENUM('pending', 'verified', 'rejected') DEFAULT 'pending',
    rejection_reason VARCHAR(500) NULL COMMENT 'Lý do từ chối nếu trạng thái bị từ chối',
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    verified_at TIMESTAMP NULL,
    verified_by BIGINT UNSIGNED NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_product (product_id),
    INDEX idx_status (status),
    INDEX idx_uploaded (uploaded_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Chứng chỉ bền vững do sản phẩm gắn kết để xác minh';

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
    UNIQUE KEY unique_vendor_bank_account (vendor_id, account_number),
    INDEX idx_vendor (vendor_id),
    INDEX idx_vendor_default (vendor_id, is_default)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Tài khoản ngân hàng của các hồ sơ nhà cung cấp';

-- =====================================================
-- CÁC BẢNG DỮ LIỆU MÔI TRƯỜNG
-- =====================================================

-- Hồ sơ trang trại cho nông dân
CREATE TABLE farm_profiles (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL UNIQUE,
    farm_name VARCHAR(255) NOT NULL,
    farm_size_hectares DECIMAL(10,2),
    location_address TEXT,
    province VARCHAR(100),
    district VARCHAR(100),
    commune VARCHAR(100),
    primary_crops VARCHAR(500) COMMENT 'Các loại cây trồng chính, danh sách phân cách bằng dấu phẩy',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_location (province, district),
    INDEX idx_farm_size (farm_size_hectares),
    INDEX idx_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Chi tiết hồ sơ trang trại cho người dùng nông dân';

-- Dữ liệu giám sát môi trường
CREATE TABLE environmental_data (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    farm_profile_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    measurement_date DATE NOT NULL,
    soil_ph DECIMAL(3,1) CHECK (soil_ph >= 0 AND soil_ph <= 14),
    co2_footprint DECIMAL(10,2) COMMENT 'Lượng khí thải CO2 tính bằng kg',
    soil_moisture_percentage DECIMAL(5,2),
    soil_type ENUM('Đất phù sa', 'Đất đỏ Bazan', 'Đất Feralit', 'Đất thịt', 'Đất sét', 'Đất cát') NOT NULL,
    notes VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (farm_profile_id) REFERENCES farm_profiles(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_farm_date (farm_profile_id, measurement_date),
    INDEX idx_user (user_id),
    INDEX idx_date (measurement_date),
    INDEX idx_soil_type (soil_type)
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
    
    FOREIGN KEY (environmental_data_id) REFERENCES environmental_data(id) ON DELETE CASCADE,
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
    
    FOREIGN KEY (environmental_data_id) REFERENCES environmental_data(id) ON DELETE CASCADE,
    INDEX idx_environmental_data (environmental_data_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Dữ liệu sử dụng năng lượng để tính toán lượng khí thải CO2';

-- =====================================================
-- CÁC BẢNG CHATBOT AI
-- =====================================================

-- Các cuộc hội thoại chatbot
CREATE TABLE chatbot_conversations (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    session_id VARCHAR(255) NOT NULL,
    title VARCHAR(255),
    context TEXT COMMENT 'Bối cảnh cuộc hội thoại và metadata',
    is_active BOOLEAN DEFAULT TRUE,
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ended_at TIMESTAMP NULL,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_user_session (user_id, session_id),
    INDEX idx_active (is_active),
    INDEX idx_started (started_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Các phiên hội thoại chatbot';

-- Tin nhắn chatbot
CREATE TABLE chatbot_messages (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    conversation_id BIGINT UNSIGNED NOT NULL,
    message_type ENUM('user', 'bot', 'system') NOT NULL,
    message_text TEXT NOT NULL,
    attachments VARCHAR(1000) COMMENT 'URL đính kèm hình ảnh hoặc tập tin, phân cách bằng dấu phẩy',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (conversation_id) REFERENCES chatbot_conversations(id) ON DELETE CASCADE,
    INDEX idx_conversation (conversation_id),
    INDEX idx_type (message_type),
    INDEX idx_created (created_at)
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
    category_id BIGINT UNSIGNED NOT NULL,
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
    last_activity_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_category (category_id),
    INDEX idx_user (user_id),
    INDEX idx_slug (slug),
    INDEX idx_status_pinned (status, is_pinned),
    INDEX idx_last_activity (last_activity_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bài viết thảo luận diễn đàn';

-- Bình luận diễn đàn
CREATE TABLE forum_comments (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    post_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    parent_id BIGINT UNSIGNED NULL COMMENT 'Dành cho bình luận lồng nhau',
    content TEXT NOT NULL,
    like_count INT DEFAULT 0,
    dislike_count INT DEFAULT 0,
    status ENUM('visible', 'moderated', 'deleted') DEFAULT 'visible',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_post (post_id),
    INDEX idx_user (user_id),
    INDEX idx_parent (parent_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bình luận bài viết diễn đàn';

-- =====================================================
-- CÁC BẢNG SẢN PHẨM VÀ TỒN KHO
-- =====================================================

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
    
    FOREIGN KEY (parent_id) REFERENCES product_categories(id) ON DELETE CASCADE,
    INDEX idx_parent (parent_id),
    INDEX idx_slug (slug),
    INDEX idx_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Danh mục sản phẩm theo cấp bậc';

-- Bảng sản phẩm
CREATE TABLE products (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    category_id BIGINT UNSIGNED NOT NULL,
    vendor_id BIGINT UNSIGNED NOT NULL,
    product_code VARCHAR(100) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    description TEXT,
    price DECIMAL(12,2) NOT NULL,
    cost_price DECIMAL(12,2) DEFAULT 0.00 COMMENT 'Giá vốn để tính toán lợi nhuận',
    commission_rate DECIMAL(5,2) DEFAULT 0.00 COMMENT 'Tỷ lệ hoa hồng cho nhà cung cấp gốc (0% = mua đầy đủ)',
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
    total_reviews INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (category_id) REFERENCES product_categories(id) ON DELETE RESTRICT,
    INDEX idx_category (category_id),
    INDEX idx_product_code (product_code),
    INDEX idx_slug (slug),
    INDEX idx_price (price),
    INDEX idx_active (is_active),
    FULLTEXT idx_search (name, description)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Sản phẩm thiết bị nông nghiệp xanh trong kho công ty';

CREATE TABLE product_registrations (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL,
    category_id BIGINT UNSIGNED NOT NULL,
    product_code VARCHAR(100) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    price DECIMAL(12,2) NOT NULL,
    commission_rate DECIMAL(5,2) DEFAULT 0.00 COMMENT 'Tỷ lệ hoa hồng mà nhà cung cấp gốc mong muốn (0% = mua đầy đủ)',
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
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    reviewed_at TIMESTAMP NULL,

    INDEX idx_vendor_status (vendor_id, status),
    INDEX idx_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Product registrations';

-- Bảng giỏ hàng
CREATE TABLE cart (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_user (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Giỏ hàng cho người dùng';

CREATE TABLE cart_items (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    cart_id BIGINT UNSIGNED NOT NULL,
    product_id BIGINT UNSIGNED NOT NULL,
    quantity INT NOT NULL DEFAULT 1 CHECK (quantity > 0),
    unit_price DECIMAL(12,2) NOT NULL COMMENT 'Price at the time of adding to cart',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (cart_id) REFERENCES cart(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    UNIQUE KEY unique_cart_product (cart_id, product_id),
    INDEX idx_cart (cart_id),
    INDEX idx_product (product_id),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cart items for customer shopping carts';

-- Theo dõi tồn kho nhập (hàng vào)
CREATE TABLE batch_inventory (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    sku VARCHAR(100) NOT NULL COMMENT 'Mã quản lý kho - mã nhận dạng duy nhất cho lô hàng này',
    vendor_profile_id BIGINT UNSIGNED NULL COMMENT 'Nhà cung cấp đã cung cấp sản phẩm (nếu có)',
    batch_number VARCHAR(100) NULL COMMENT 'Số lô hoặc số lô hàng để theo dõi',
    lot_number VARCHAR(100) NULL COMMENT 'Số lô sản xuất',
    quantity INT NOT NULL,
    unit_cost_price DECIMAL(12,2) NOT NULL COMMENT 'Giá vốn mỗi đơn vị khi mua',
    expiry_date DATE NULL COMMENT 'Ngày hết hạn sản phẩm nếu có',
    manufacturing_date DATE NULL COMMENT 'Ngày sản xuất nếu có',
    quality_check_status ENUM('pending', 'passed', 'failed', 'not_required') DEFAULT 'not_required' COMMENT 'Trạng thái kiểm tra chất lượng',
    quality_checked_by BIGINT UNSIGNED NULL COMMENT 'Người thực hiện kiểm tra chất lượng',
    quality_checked_at TIMESTAMP NULL COMMENT 'Khi nào kiểm tra chất lượng được thực hiện',
    notes VARCHAR(500) NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_product (product_id),
    INDEX idx_sku (sku),
    INDEX idx_vendor (vendor_profile_id),
    INDEX idx_expiry_date (expiry_date),
    INDEX idx_quality_status (quality_check_status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Theo dõi tồn kho nhập - hàng vào với thông tin nhận hàng chi tiết';

-- Theo dõi tồn kho bán hàng (hàng ra)
CREATE TABLE export_inventory (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    order_id BIGINT UNSIGNED NULL COMMENT 'Đơn hàng gây ra chuyển động tồn kho này',
    quantity INT NOT NULL COMMENT 'Số lượng bán (âm cho hàng trả)',
    unit_sale_price DECIMAL(12,2) NOT NULL COMMENT 'Giá bán mỗi đơn vị',
    balance_after INT NOT NULL COMMENT 'Số dư tồn kho sau lần bán này',
    movement_type ENUM('sale', 'return', 'damage', 'loss', 'adjustment') DEFAULT 'sale',
    notes VARCHAR(500) NULL,
    created_by BIGINT UNSIGNED NOT NULL COMMENT 'Người dùng ghi nhận chuyển động này',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_product (product_id),
    INDEX idx_order (order_id),
    INDEX idx_movement_type (movement_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Theo dõi tồn kho bán hàng - hàng ra';

-- =====================================================
-- CÁC BẢNG QUẢN LÝ YÊU CẦU
-- =====================================================

-- Bảng yêu cầu tổng quát cho các loại yêu cầu khác nhau
CREATE TABLE requests (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NULL,
    request_type ENUM('refund_request', 'payout_request', 'support_request', 'vendor_register') NOT NULL,
    title VARCHAR(255) NOT NULL COMMENT 'Tiêu đề/chủ đề yêu cầu',
    description TEXT NOT NULL COMMENT 'Mô tả chi tiết về yêu cầu',
    status ENUM('pending', 'in_review', 'approved', 'rejected', 'completed', 'cancelled') DEFAULT 'pending',
    amount DECIMAL(12,2) NULL COMMENT 'Số tiền liên quan (cho yêu cầu hoàn tiền/thanh toán)',
    admin_notes TEXT NULL COMMENT 'Ghi chú nội bộ từ admin/nhân viên',
    rejection_reason VARCHAR(500) NULL COMMENT 'Lý do từ chối nếu trạng thái bị từ chối',
    processed_by BIGINT UNSIGNED NULL COMMENT 'Admin/nhân viên đã xử lý yêu cầu',
    processed_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_type_status (request_type, status),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Hệ thống quản lý yêu cầu tổng quát';

-- =====================================================
-- CÁC BẢNG ĐƠN HÀNG VÀ THANH TOÁN
-- =====================================================

-- Bảng đơn hàng
CREATE TABLE orders (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    status ENUM('pending', 'confirmed', 'processing', 'shipped', 'delivered', 'cancelled', 'refunded') DEFAULT 'pending',
    subtotal DECIMAL(12,2) NOT NULL,
    tax_amount DECIMAL(12,2) DEFAULT 0.00,
    shipping_fee DECIMAL(12,2) DEFAULT 0.00,
    discount_amount DECIMAL(12,2) DEFAULT 0.00,
    total_amount DECIMAL(12,2) NOT NULL,
    shipping_address JSON NOT NULL,
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
    
    INDEX idx_status (status),
    INDEX idx_created_at (created_at)
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
    
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    INDEX idx_order (order_id),
    INDEX idx_product (product_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Các mục trong đơn hàng';

-- =====================================================
-- CÁC BẢNG HỆ THỐNG TÀI CHÍNH
-- =====================================================

-- Ví cho nhà cung cấp (một ví cho một nhà cung cấp)
CREATE TABLE wallets (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL UNIQUE,
    balance DECIMAL(12,2) NOT NULL DEFAULT 0.00 COMMENT 'Số dư khả dụng',
    pending_withdraw DECIMAL(12,2) NOT NULL DEFAULT 0.00 COMMENT 'Số tiền yêu cầu rút, đang chờ xử lý',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Ví nhà cung cấp theo dõi số dư';

-- Bảng giao dịch (sổ cái trung tâm - nguồn sự thật duy nhất cho tất cả các chuyển động tài chính)
CREATE TABLE transactions (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    transaction_type ENUM('payment_in', 'cashout', 'wallet_credit', 'wallet_debit', 'commission', 'refund', 'adjustment') NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'VND',
    
    -- Tham chiếu cốt lõi
    order_id BIGINT UNSIGNED NULL COMMENT 'Tham chiếu đến bảng đơn hàng',
    customer_id BIGINT UNSIGNED NULL COMMENT 'Khách hàng liên quan đến giao dịch',
    vendor_id BIGINT UNSIGNED NULL COMMENT 'Nhà cung cấp liên quan đến giao dịch',
    
    -- Các trường liên quan đến ví
    wallet_id BIGINT UNSIGNED NULL COMMENT 'Tham chiếu đến bảng ví',
    balance_before DECIMAL(12,2) NULL COMMENT 'Số dư ví trước giao dịch',
    balance_after DECIMAL(12,2) NULL COMMENT 'Số dư ví sau giao dịch',
    
    -- Trạng thái và metadata
    status ENUM('pending','completed','failed','cancelled') NOT NULL DEFAULT 'pending',
    description VARCHAR(255) NOT NULL COMMENT 'Mô tả có thể đọc được',
    metadata JSON NULL COMMENT 'Metadata bổ sung của giao dịch',
    
    -- Tham chiếu đến các bảng cụ thể theo lĩnh vực (thanh toán/rút tiền sẽ tham chiếu ngược lại đến bảng này)
    reference_type VARCHAR(50) NULL COMMENT 'Loại thực thể tham chiếu bổ sung',
    reference_id BIGINT UNSIGNED NULL COMMENT 'ID của thực thể tham chiếu bổ sung',
    
    -- Các trường kiểm toán
    created_by BIGINT UNSIGNED NULL COMMENT 'Người dùng khởi tạo giao dịch này',
    processed_by BIGINT UNSIGNED NULL COMMENT 'Người dùng xử lý giao dịch này',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE RESTRICT,
    FOREIGN KEY (wallet_id) REFERENCES wallets(id) ON DELETE RESTRICT,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (processed_by) REFERENCES users(id) ON DELETE RESTRICT,
    
    INDEX idx_type_status (transaction_type, status),
    INDEX idx_order (order_id),
    INDEX idx_customer (customer_id),
    INDEX idx_vendor (vendor_id),
    INDEX idx_wallet (wallet_id),
    INDEX idx_created_at (created_at),
    INDEX idx_completed_at (completed_at),
    INDEX idx_reference (reference_type, reference_id),
    INDEX idx_amount (amount),
    INDEX idx_created_by (created_by)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Sổ cái tài chính trung tâm - nguồn sự thật duy nhất cho tất cả chuyển động tiền tệ';

-- Bảng thanh toán
CREATE TABLE payments (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    order_id BIGINT UNSIGNED NOT NULL,
    transaction_id BIGINT UNSIGNED NULL COMMENT 'Tham chiếu đến bảng giao dịch để đảm bảo tính nhất quán',
    payment_method ENUM('credit_card', 'debit_card', 'paypal', 'stripe', 'bank_transfer', 'cod') NOT NULL,
    payment_gateway ENUM('stripe', 'paypal', 'vnpay', 'momo', 'manual') NOT NULL,
    gateway_transaction_id VARCHAR(255) UNIQUE COMMENT 'ID giao dịch từ cổng thanh toán',
    amount DECIMAL(12,2) NOT NULL,
    status ENUM('pending', 'processing', 'completed', 'failed', 'refunded', 'partially_refunded') DEFAULT 'pending',
    gateway_response JSON COMMENT 'Phản hồi thô từ cổng thanh toán',
    refund_amount DECIMAL(12,2) DEFAULT 0.00,
    refund_reason VARCHAR(500),
    refunded_at TIMESTAMP NULL,
    paid_at TIMESTAMP NULL,
    failed_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE RESTRICT,
    INDEX idx_order (order_id),
    INDEX idx_transaction (transaction_id),
    INDEX idx_gateway_transaction (gateway_transaction_id),
    INDEX idx_status (status),
    INDEX idx_payment_method (payment_method)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Các giao dịch thanh toán với chi tiết cổng thanh toán';

-- Bảng rút tiền (tiền ra - thanh toán cho nhà cung cấp, chi phí)
CREATE TABLE cashouts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL,
    transaction_id BIGINT UNSIGNED NULL COMMENT 'Tham chiếu đến bảng giao dịch để đảm bảo tính nhất quán',
    amount DECIMAL(12,2) NOT NULL,
    bank_code VARCHAR(20) NOT NULL,
    bank_account_number VARCHAR(50) NOT NULL,
    bank_account_holder VARCHAR(255) NOT NULL,
    status ENUM('pending','processing','completed','failed','cancelled') NOT NULL DEFAULT 'pending',
    cashout_type ENUM('commission_payout', 'vendor_payment', 'expense', 'refund') DEFAULT 'commission_payout',
    gateway_transaction_id VARCHAR(255) NULL COMMENT 'ID giao dịch cổng thanh toán bên ngoài',
    reference_type VARCHAR(50) NULL COMMENT 'Loại tham chiếu (đơn hàng, yêu cầu, v.v.)',
    reference_id BIGINT UNSIGNED NULL COMMENT 'ID của thực thể tham chiếu',
    notes VARCHAR(500) NULL,
    processed_by BIGINT UNSIGNED NULL COMMENT 'Admin đã xử lý lần rút tiền này',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMP NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    UNIQUE KEY idx_unique_gateway_transaction (gateway_transaction_id),
    INDEX idx_vendor (vendor_id),
    INDEX idx_status (status),
    INDEX idx_type (cashout_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Tiền ra - thanh toán cho nhà cung cấp và chi phí với chi tiết ngân hàng';


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
    
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE CASCADE,
    UNIQUE KEY unique_product_order_customer (product_id, order_id, customer_id),
    INDEX idx_product_rating (product_id, rating),
    INDEX idx_customer (customer_id),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Đánh giá và xếp hạng sản phẩm';

-- =====================================================
-- CÁC INDEX BỔ SUNG ĐỂ TỐI ƯU HÓA HIỆU SUẤT
-- =====================================================
CREATE INDEX idx_products_search ON products(is_active, category_id, price);
CREATE INDEX idx_orders_date_range ON orders(created_at, status);
CREATE INDEX idx_env_data_analysis ON environmental_data(farm_profile_id, measurement_date, soil_ph);

-- =======================================================
-- THÊM TẤT CẢ FOREIGN KEY CONSTRAINTS
-- =======================================================

-- Thêm FK cho vendor_profiles
ALTER TABLE vendor_profiles 
ADD CONSTRAINT fk_vendor_profiles__user_id__users__id 
FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;

ALTER TABLE vendor_profiles 
ADD CONSTRAINT fk_vendor_profiles__verified_by__users__id 
FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE RESTRICT;

-- Thêm FK cho vendor_bank_accounts
ALTER TABLE vendor_bank_accounts 
ADD CONSTRAINT fk_vendor_bank_accounts__vendor_id__vendor_profiles__id 
FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE CASCADE;

-- Thêm FK cho vendor_certificates
ALTER TABLE vendor_certificates 
ADD CONSTRAINT fk_vendor_certificates__vendor_id__vendor_profiles__id 
FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE CASCADE;

ALTER TABLE vendor_certificates 
ADD CONSTRAINT fk_vendor_certificates__verified_by__users__id 
FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE RESTRICT;

-- Thêm FK cho product_certificates  
ALTER TABLE product_certificates 
ADD CONSTRAINT fk_product_certificates__product_id__products__id 
FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE;

ALTER TABLE product_certificates 
ADD CONSTRAINT fk_product_certificates__verified_by__users__id 
FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE RESTRICT;

-- Thêm FK cho farm_profiles
ALTER TABLE farm_profiles 
ADD CONSTRAINT fk_farm_profiles__user_id__users__id 
FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;

-- Thêm FK cho product_categories (self-referencing)
ALTER TABLE product_categories 
ADD CONSTRAINT fk_product_categories__parent_id__product_categories__id 
FOREIGN KEY (parent_id) REFERENCES product_categories(id) ON DELETE CASCADE;

-- Thêm FK cho products
ALTER TABLE products 
ADD CONSTRAINT fk_products__category_id__product_categories__id 
FOREIGN KEY (category_id) REFERENCES product_categories(id) ON DELETE RESTRICT;

ALTER TABLE products 
ADD CONSTRAINT fk_products__vendor_id__vendor_profiles__id 
FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE RESTRICT;

-- Thêm FK cho product_registrations
ALTER TABLE product_registrations 
ADD CONSTRAINT fk_product_registrations__vendor_id__vendor_profiles__id 
FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE CASCADE;

ALTER TABLE product_registrations 
ADD CONSTRAINT fk_product_registrations__category_id__product_categories__id 
FOREIGN KEY (category_id) REFERENCES product_categories(id) ON DELETE RESTRICT;

-- Thêm FK cho cart
ALTER TABLE cart 
ADD CONSTRAINT fk_cart__user_id__users__id 
FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;

-- Thêm FK cho cart_items
ALTER TABLE cart_items 
ADD CONSTRAINT fk_cart_items__cart_id__cart__id 
FOREIGN KEY (cart_id) REFERENCES cart(id) ON DELETE CASCADE;

ALTER TABLE cart_items 
ADD CONSTRAINT fk_cart_items__product_id__products__id 
FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE;

-- Thêm FK cho batch_inventory
ALTER TABLE batch_inventory 
ADD CONSTRAINT fk_batch_inventory__product_id__products__id 
FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE;

ALTER TABLE batch_inventory 
ADD CONSTRAINT fk_batch_inventory__vendor_profile_id__vendor_profiles__id 
FOREIGN KEY (vendor_profile_id) REFERENCES vendor_profiles(id) ON DELETE RESTRICT;

-- Thêm FK cho export_inventory
ALTER TABLE export_inventory 
ADD CONSTRAINT fk_export_inventory__product_id__products__id 
FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE;

ALTER TABLE export_inventory 
ADD CONSTRAINT fk_export_inventory__order_id__orders__id 
FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT;

-- Thêm FK cho requests
ALTER TABLE requests 
ADD CONSTRAINT fk_requests__user_id__users__id 
FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;

-- Thêm FK cho orders
ALTER TABLE orders 
ADD CONSTRAINT fk_orders__user_id__users__id 
FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT;

-- Thêm FK cho order_details
ALTER TABLE order_details 
ADD CONSTRAINT fk_order_details__order_id__orders__id 
FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE;

ALTER TABLE order_details 
ADD CONSTRAINT fk_order_details__product_id__products__id 
FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT;

-- Thêm FK cho wallets
ALTER TABLE wallets 
ADD CONSTRAINT fk_wallets__vendor_id__vendor_profiles__id 
FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE CASCADE;

-- Thêm FK cho transactions
ALTER TABLE transactions 
ADD CONSTRAINT fk_transactions__order_id__orders__id 
FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT;

ALTER TABLE transactions 
ADD CONSTRAINT fk_transactions__customer_id__users__id 
FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT;

ALTER TABLE transactions 
ADD CONSTRAINT fk_transactions__vendor_id__vendor_profiles__id 
FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE RESTRICT;

ALTER TABLE transactions 
ADD CONSTRAINT fk_transactions__wallet_id__wallets__id 
FOREIGN KEY (wallet_id) REFERENCES wallets(id) ON DELETE RESTRICT;

ALTER TABLE transactions 
ADD CONSTRAINT fk_transactions__created_by__users__id 
FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT;

ALTER TABLE transactions 
ADD CONSTRAINT fk_transactions__processed_by__users__id 
FOREIGN KEY (processed_by) REFERENCES users(id) ON DELETE RESTRICT;

-- Thêm FK cho payments
ALTER TABLE payments 
ADD CONSTRAINT fk_payments__order_id__orders__id 
FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT;

ALTER TABLE payments 
ADD CONSTRAINT fk_payments__transaction_id__transactions__id 
FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE RESTRICT;

-- Thêm FK cho cashouts
ALTER TABLE cashouts 
ADD CONSTRAINT fk_cashouts__vendor_id__vendor_profiles__id 
FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE RESTRICT;

ALTER TABLE cashouts 
ADD CONSTRAINT fk_cashouts__transaction_id__transactions__id 
FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE RESTRICT;

ALTER TABLE cashouts 
ADD CONSTRAINT fk_cashouts__processed_by__users__id 
FOREIGN KEY (processed_by) REFERENCES users(id) ON DELETE RESTRICT;

-- Thêm FK cho forum_posts
ALTER TABLE forum_posts 
ADD CONSTRAINT fk_forum_posts__category_id__forum_categories__id 
FOREIGN KEY (category_id) REFERENCES forum_categories(id) ON DELETE CASCADE;

ALTER TABLE forum_posts 
ADD CONSTRAINT fk_forum_posts__user_id__users__id 
FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;

-- Thêm FK cho forum_comments
ALTER TABLE forum_comments 
ADD CONSTRAINT fk_forum_comments__post_id__forum_posts__id 
FOREIGN KEY (post_id) REFERENCES forum_posts(id) ON DELETE CASCADE;

ALTER TABLE forum_comments 
ADD CONSTRAINT fk_forum_comments__user_id__users__id 
FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;

ALTER TABLE forum_comments 
ADD CONSTRAINT fk_forum_comments__parent_id__forum_comments__id 
FOREIGN KEY (parent_id) REFERENCES forum_comments(id) ON DELETE CASCADE;

-- Thêm FK cho product_reviews
ALTER TABLE product_reviews 
ADD CONSTRAINT fk_product_reviews__product_id__products__id 
FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE;

ALTER TABLE product_reviews 
ADD CONSTRAINT fk_product_reviews__order_id__orders__id 
FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE;

ALTER TABLE product_reviews 
ADD CONSTRAINT fk_product_reviews__customer_id__users__id 
FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE CASCADE;

-- Thêm FK cho chatbot_conversations
ALTER TABLE chatbot_conversations 
ADD CONSTRAINT fk_chatbot_conversations__user_id__users__id 
FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;

-- Thêm FK cho chatbot_messages
ALTER TABLE chatbot_messages 
ADD CONSTRAINT fk_chatbot_messages__conversation_id__chatbot_conversations__id 
FOREIGN KEY (conversation_id) REFERENCES chatbot_conversations(id) ON DELETE CASCADE;

-- Thêm FK cho environmental_data
ALTER TABLE environmental_data 
ADD CONSTRAINT fk_environmental_data__farm_profile_id__farm_profiles__id 
FOREIGN KEY (farm_profile_id) REFERENCES farm_profiles(id) ON DELETE CASCADE;

ALTER TABLE environmental_data 
ADD CONSTRAINT fk_environmental_data__user_id__users__id 
FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE;

-- Thêm FK cho fertilizers
ALTER TABLE fertilizers 
ADD CONSTRAINT fk_fertilizers__environmental_data_id__environmental_data__id 
FOREIGN KEY (environmental_data_id) REFERENCES environmental_data(id) ON DELETE CASCADE;

-- Thêm FK cho energy_usage
ALTER TABLE energy_usage 
ADD CONSTRAINT fk_energy_usage__environmental_data_id__environmental_data__id 
FOREIGN KEY (environmental_data_id) REFERENCES environmental_data(id) ON DELETE CASCADE;

-- =======================================================
-- HOÀN THÀNH - TẤT CẢ FK HỢP LỆ ĐÃ ĐƯỢC THÊM
-- =======================================================

-- Lược đồ cơ sở dữ liệu hoàn thành
-- Phiên bản 7.0 - Refactor và tối ưu hóa cấu trúc cơ sở dữ liệu
-- Tổng số bảng: 29
-- Thay đổi từ v6.0:

-- BẢNG BỊ XÓA:
-- - Xóa bảng sustainability_certifications (không sử dụng)
-- - Xóa bảng supported_banks (không sử dụng)
-- - Xóa bảng weather_data_cache (không sử dụng)

-- BẢNG ĐỔI TÊN:
-- - vendor_sustainability_credentials → vendor_certificates
-- - product_sustainability_credentials → product_certificates
-- - purchase_inventory → batch_inventory
-- - sales_inventory → export_inventory

-- CỘT BỊ XÓA KHỎI BẢNG HIỆN CÓ:
-- vendor_profiles: tax_code, commission_rate, rating_average, total_reviews
-- farm_profiles: latitude, longitude
-- product_categories: name_en
-- products: name_en, description_en, is_featured
-- forum_posts: reply_count, is_locked, moderated_reason, moderated_by (cột)
-- forum_comments: moderated_reason, moderated_by (cột)
-- product_reviews: helpful_count, unhelpful_count

-- CỘT THAY ĐỔI:
-- vendor_bank_accounts: bank_id → bank_code (VARCHAR(20))
-- orders: customer_id → user_id
-- requests: requester_id → user_id

-- CẤU TRÚC MỚI:
-- - Tách cart thành cart + cart_items
-- - Đơn giản hóa batch_inventory (xóa nhiều cột tracking phức tạp)
-- - Loại bỏ các FK tham chiếu bảng không tồn tại
-- - Tối ưu index theo giới hạn 3-5 index/bảng
-- - Gộp tất cả FK constraints vào cuối file

-- TÍNH NĂNG MỚI:
-- - Cấu trúc refactor hoàn toàn để tương thích MySQL 8.0
-- - Loại bỏ vòng phụ thuộc FK
-- - Tối ưu hiệu suất với index được cân nhắc kỹ lưỡng
-- - Đảm bảo tính nhất quán dữ liệu với FK constraints hợp lệ

-- Sẵn sàng cho triển khai
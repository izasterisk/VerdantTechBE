-- VerdantTech Solutions Database Schema
-- AI-Integrated Green Agricultural Equipment Platform for Sustainable Vegetable Farming
-- Version: 5.0
-- Engine: InnoDB (for transaction support)
-- Character Set: utf8mb4 (for multilingual support)

-- Create database
CREATE DATABASE IF NOT EXISTS verdanttech_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE verdanttech_db;

-- =====================================================
-- USER MANAGEMENT TABLES
-- =====================================================

-- Users table (base authentication)
CREATE TABLE users (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role ENUM('customer', 'seller', 'vendor', 'admin', 'manager') NOT NULL DEFAULT 'customer',
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Base user authentication and profile table';

-- Vendor profiles for sellers
CREATE TABLE vendor_profiles (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL UNIQUE,
    company_name VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    business_registration_number VARCHAR(100) UNIQUE,
    tax_code VARCHAR(50),
    company_address TEXT,
    verified_at TIMESTAMP NULL,
    verified_by BIGINT UNSIGNED NULL,
    commission_rate DECIMAL(5,2) DEFAULT 10.00 COMMENT 'Platform commission percentage',
    rating_average DECIMAL(3,2) DEFAULT 0.00,
    total_reviews INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE SET NULL,
    INDEX idx_company_name (company_name),
    INDEX idx_slug (slug),
    INDEX idx_verified (verified_at),
    INDEX idx_rating (rating_average)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Vendor/seller profile and verification details';

-- Sustainability certifications reference table
CREATE TABLE sustainability_certifications (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(100) NOT NULL UNIQUE COMMENT 'Certification code (e.g., GLOBALGAP, USDA_ORGANIC)',
    name VARCHAR(255) NOT NULL COMMENT 'Full certification name',
    category ENUM('organic', 'environmental', 'fair_trade', 'food_safety', 'social', 'energy') NOT NULL,
    issuing_body VARCHAR(255) NULL COMMENT 'Organization that issues the certification',
    description TEXT COMMENT 'Detailed description of the certification',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_code (code),
    INDEX idx_category (category),
    INDEX idx_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Master list of sustainability certifications';

-- Vendor sustainability credentials (uploaded certificates)
CREATE TABLE vendor_sustainability_credentials (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL,
    certification_id BIGINT UNSIGNED NOT NULL,
    certificate_url VARCHAR(500) NOT NULL COMMENT 'URL to uploaded certificate image/file',
    status ENUM('pending', 'verified', 'rejected') DEFAULT 'pending',
    rejection_reason VARCHAR(500) NULL COMMENT 'Reason for rejection if status is rejected',
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    verified_at TIMESTAMP NULL,
    verified_by BIGINT UNSIGNED NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE CASCADE,
    FOREIGN KEY (certification_id) REFERENCES sustainability_certifications(id) ON DELETE RESTRICT,
    FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE SET NULL,
    UNIQUE KEY unique_vendor_certification (vendor_id, certification_id),
    INDEX idx_vendor (vendor_id),
    INDEX idx_certification (certification_id),
    INDEX idx_status (status),
    INDEX idx_uploaded (uploaded_at),
    INDEX idx_verified (verified_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Vendor uploaded sustainability certificates for verification';

-- Supported banks master list
CREATE TABLE supported_banks (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    bank_code VARCHAR(20) NOT NULL UNIQUE COMMENT 'Short bank code, e.g., VCB, TCB',
    bank_name VARCHAR(255) NOT NULL COMMENT 'Full bank name, e.g., Vietcombank',
    image_url VARCHAR(500) NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='List of supported banks';

-- Vendor bank accounts (one vendor may have multiple bank accounts)
CREATE TABLE vendor_bank_accounts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL,
    bank_id BIGINT UNSIGNED NOT NULL,
    account_number VARCHAR(50) NOT NULL,
    account_holder VARCHAR(255) NOT NULL,
    is_default BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE CASCADE,
    FOREIGN KEY (bank_id) REFERENCES supported_banks(id) ON DELETE RESTRICT,
    UNIQUE KEY unique_vendor_bank_account (vendor_id, bank_id, account_number),
    INDEX idx_vendor (vendor_id),
    INDEX idx_bank (bank_id),
    INDEX idx_vendor_default (vendor_id, is_default)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bank accounts of vendor profiles';

-- =====================================================
-- ENVIRONMENTAL DATA TABLES
-- =====================================================

-- Farm profiles for farmers
CREATE TABLE farm_profiles (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL UNIQUE,
    farm_name VARCHAR(255) NOT NULL,
    farm_size_hectares DECIMAL(10,2),
    location_address TEXT,
    province VARCHAR(100),
    district VARCHAR(100),
    commune VARCHAR(100),
    latitude DECIMAL(10,8),
    longitude DECIMAL(11,8),
    primary_crops VARCHAR(500) COMMENT 'Main crops grown, comma-separated list',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_location (province, district),
    INDEX idx_farm_size (farm_size_hectares)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Farm profile details for farmer users';

-- Environmental monitoring data
CREATE TABLE environmental_data (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    farm_profile_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    measurement_date DATE NOT NULL,
    soil_ph DECIMAL(3,1) CHECK (soil_ph >= 0 AND soil_ph <= 14),
    co2_footprint DECIMAL(10,2) COMMENT 'CO2 emissions in kg',
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Manual environmental data input by farmers';

-- Fertilizer usage tracking
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Fertilizer usage data for CO2 footprint calculation';

-- Energy usage tracking
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Energy usage data for CO2 footprint calculation';

-- Weather data cache
CREATE TABLE weather_data_cache (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    farm_profile_id BIGINT UNSIGNED NOT NULL,
    api_source ENUM('openweathermap', 'accuweather') NOT NULL,
    weather_date DATE NOT NULL,
    temperature_min DECIMAL(5,2),
    temperature_max DECIMAL(5,2),
    temperature_avg DECIMAL(5,2),
    humidity_percentage DECIMAL(5,2),
    precipitation_mm DECIMAL(8,2),
    wind_speed_kmh DECIMAL(6,2),
    wind_direction VARCHAR(10),
    uv_index DECIMAL(3,1),
    weather_condition VARCHAR(100),
    weather_icon VARCHAR(50),
    sunrise_time TIME,
    sunset_time TIME,
    raw_api_response JSON,
    fetched_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (farm_profile_id) REFERENCES farm_profiles(id) ON DELETE CASCADE,
    UNIQUE KEY unique_farm_date_source (farm_profile_id, weather_date, api_source),
    INDEX idx_farm_date (farm_profile_id, weather_date),
    INDEX idx_fetched (fetched_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Cached weather data from external APIs';

-- =====================================================
-- AI CHATBOT TABLES
-- =====================================================

-- Chatbot conversations
CREATE TABLE chatbot_conversations (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    session_id VARCHAR(255) NOT NULL,
    title VARCHAR(255),
    context TEXT COMMENT 'Conversation context and metadata',
    is_active BOOLEAN DEFAULT TRUE,
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ended_at TIMESTAMP NULL,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_user_session (user_id, session_id),
    INDEX idx_active (is_active),
    INDEX idx_started (started_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Chatbot conversation sessions';

-- Chatbot messages
CREATE TABLE chatbot_messages (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    conversation_id BIGINT UNSIGNED NOT NULL,
    message_type ENUM('user', 'bot', 'system') NOT NULL,
    message_text TEXT NOT NULL,
    attachments VARCHAR(1000) COMMENT 'Image or file attachment URLs, comma-separated',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (conversation_id) REFERENCES chatbot_conversations(id) ON DELETE CASCADE,
    INDEX idx_conversation (conversation_id),
    INDEX idx_type (message_type),
    INDEX idx_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Individual chatbot messages';

-- =====================================================
-- COMMUNITY TABLES
-- =====================================================

-- Forum categories
CREATE TABLE forum_categories (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Forum discussion categories';

-- Forum posts
CREATE TABLE forum_posts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    category_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    content JSON NOT NULL COMMENT 'Mixed content blocks: [{"order": 1, "type": "text", "content": "Hello world"}, {"order": 2, "type": "image", "content": "https://example.com/image.jpg"}]',
    tags VARCHAR(500) COMMENT 'Tags, comma-separated list',
    view_count BIGINT DEFAULT 0,
    reply_count INT DEFAULT 0,
    like_count INT DEFAULT 0,
    dislike_count INT DEFAULT 0,
    is_pinned BOOLEAN DEFAULT FALSE,
    is_locked BOOLEAN DEFAULT FALSE,
    status ENUM('published', 'draft', 'moderated', 'deleted') DEFAULT 'published',
    moderated_reason VARCHAR(500),
    moderated_by BIGINT UNSIGNED NULL,
    last_activity_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (category_id) REFERENCES forum_categories(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (moderated_by) REFERENCES users(id) ON DELETE SET NULL,
    INDEX idx_category (category_id),
    INDEX idx_user (user_id),
    INDEX idx_slug (slug),
    INDEX idx_status_pinned (status, is_pinned),
    INDEX idx_last_activity (last_activity_at),
    FULLTEXT idx_search (title)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Forum discussion posts';

-- Forum comments
CREATE TABLE forum_comments (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    post_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    parent_id BIGINT UNSIGNED NULL COMMENT 'For nested comments',
    content TEXT NOT NULL,
    like_count INT DEFAULT 0,
    dislike_count INT DEFAULT 0,
    status ENUM('visible', 'moderated', 'deleted') DEFAULT 'visible',
    moderated_reason VARCHAR(500),
    moderated_by BIGINT UNSIGNED NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (post_id) REFERENCES forum_posts(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (parent_id) REFERENCES forum_comments(id) ON DELETE CASCADE,
    FOREIGN KEY (moderated_by) REFERENCES users(id) ON DELETE SET NULL,
    INDEX idx_post (post_id),
    INDEX idx_user (user_id),
    INDEX idx_parent (parent_id),
    INDEX idx_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Forum post comments';

-- =====================================================
-- PRODUCT AND INVENTORY TABLES
-- =====================================================

-- Product categories
CREATE TABLE product_categories (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    parent_id BIGINT UNSIGNED NULL,
    name VARCHAR(255) NOT NULL,
    name_en VARCHAR(255),
    slug VARCHAR(255) UNIQUE NOT NULL,
    description TEXT,
    icon_url VARCHAR(500),
    sort_order INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (parent_id) REFERENCES product_categories(id) ON DELETE CASCADE,
    INDEX idx_parent (parent_id),
    INDEX idx_slug (slug),
    INDEX idx_active_sort (is_active, sort_order)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Hierarchical product categories';

-- Products table
CREATE TABLE products (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    category_id BIGINT UNSIGNED NOT NULL,
    product_code VARCHAR(100) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    name_en VARCHAR(255),
    slug VARCHAR(255) UNIQUE NOT NULL,
    description TEXT,
    description_en TEXT,
    price DECIMAL(12,2) NOT NULL,
    cost_price DECIMAL(12,2) DEFAULT 0.00 COMMENT 'Cost price for profit calculation',
    commission_rate DECIMAL(5,2) DEFAULT 0.00 COMMENT 'Commission percentage for original vendor (0% = full purchase)',
    discount_percentage DECIMAL(5,2) DEFAULT 0.00,
    green_certifications VARCHAR(500) COMMENT 'Eco certification codes, comma-separated',
    energy_efficiency_rating VARCHAR(10),
    specifications JSON COMMENT 'Technical specifications as key-value pairs',
    manual_urls VARCHAR(1000) COMMENT 'Manual/guide URLs, comma-separated',
    images VARCHAR(1000) COMMENT 'Image URLs, comma-separated',
    warranty_months INT DEFAULT 12,
    stock_quantity INT DEFAULT 0,
    weight_kg DECIMAL(10,3),
    dimensions_cm JSON COMMENT '{length, width, height}',
    is_featured BOOLEAN DEFAULT FALSE,
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
    INDEX idx_name (name),
    INDEX idx_price (price),
    INDEX idx_commission (commission_rate),
    INDEX idx_active_featured (is_active, is_featured),
    INDEX idx_rating (rating_average),
    FULLTEXT idx_search (name, name_en, description, description_en)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Green agricultural equipment products in company inventory';

-- Shopping cart table
CREATE TABLE cart (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    product_id BIGINT UNSIGNED NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_product (user_id, product_id),
    INDEX idx_user (user_id),
    INDEX idx_product (product_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Shopping cart for users';

-- Purchase inventory tracking (stock in)
CREATE TABLE purchase_inventory (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    vendor_profile_id BIGINT UNSIGNED NULL COMMENT 'Vendor who provided the products (if applicable)',
    quantity INT NOT NULL,
    unit_cost_price DECIMAL(12,2) NOT NULL COMMENT 'Cost price per unit when purchased',
    total_cost DECIMAL(12,2) NOT NULL COMMENT 'Total cost for this purchase',
    commission_rate DECIMAL(5,2) DEFAULT 0.00 COMMENT 'Commission rate for this purchase',
    batch_number VARCHAR(100) NULL COMMENT 'Batch or lot number for tracking',
    supplier_invoice VARCHAR(255) NULL COMMENT 'Supplier invoice reference',
    notes VARCHAR(500) NULL,
    balance_after INT NOT NULL COMMENT 'Stock balance after this purchase',
    created_by BIGINT UNSIGNED NOT NULL COMMENT 'User who recorded this purchase',
    purchased_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    FOREIGN KEY (vendor_profile_id) REFERENCES vendor_profiles(id) ON DELETE SET NULL,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_product (product_id),
    INDEX idx_vendor (vendor_profile_id),
    INDEX idx_purchased_date (purchased_at),
    INDEX idx_created_by (created_by),
    INDEX idx_batch (batch_number)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Purchase inventory tracking - stock coming in';

-- =====================================================
-- REQUEST MANAGEMENT TABLES
-- =====================================================

-- Generic requests table for various request types
CREATE TABLE requests (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    requester_id BIGINT UNSIGNED NOT NULL COMMENT 'User who made the request',
    request_type ENUM('refund_request', 'payout_request', 'support_request') NOT NULL,
    title VARCHAR(255) NOT NULL COMMENT 'Request title/subject',
    description TEXT NOT NULL COMMENT 'Detailed description of the request',
    status ENUM('pending', 'in_review', 'approved', 'rejected', 'completed', 'cancelled') DEFAULT 'pending',
    priority ENUM('low', 'medium', 'high', 'urgent') NULL DEFAULT 'medium',
    reference_type VARCHAR(50) NULL COMMENT 'Type of referenced entity (order, product, etc.)',
    reference_id BIGINT UNSIGNED NULL COMMENT 'ID of referenced entity',
    amount DECIMAL(12,2) NULL COMMENT 'Amount involved (for refund/payout requests)',
    admin_notes TEXT NULL COMMENT 'Internal notes from admin/staff',
    rejection_reason VARCHAR(500) NULL COMMENT 'Reason for rejection if status is rejected',
    assigned_to BIGINT UNSIGNED NULL COMMENT 'Admin/staff assigned to handle this request',
    processed_by BIGINT UNSIGNED NULL COMMENT 'Admin/staff who processed the request',
    processed_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (requester_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (assigned_to) REFERENCES users(id) ON DELETE SET NULL,
    FOREIGN KEY (processed_by) REFERENCES users(id) ON DELETE SET NULL,
    INDEX idx_requester (requester_id),
    INDEX idx_type_status (request_type, status),
    INDEX idx_priority (priority),
    INDEX idx_assigned (assigned_to),
    INDEX idx_processed (processed_by),
    INDEX idx_created_at (created_at),
    INDEX idx_reference (reference_type, reference_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Generic request management system';

-- =====================================================
-- ORDER AND PAYMENT TABLES
-- =====================================================

-- Orders table
CREATE TABLE orders (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    customer_id BIGINT UNSIGNED NOT NULL,
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
    
    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_customer (customer_id),
    INDEX idx_status (status),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Customer orders';

-- Order details
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Order line items';

-- =====================================================
-- FINANCIAL SYSTEM TABLES
-- =====================================================

-- Wallets for vendors (one wallet per vendor)
CREATE TABLE wallets (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL UNIQUE,
    balance DECIMAL(12,2) NOT NULL DEFAULT 0.00 COMMENT 'Available balance',
    pending_withdraw DECIMAL(12,2) NOT NULL DEFAULT 0.00 COMMENT 'Amount requested to withdraw, pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Vendor wallets tracking balances';

-- Transactions table (central ledger - single source of truth for all financial movements)
CREATE TABLE transactions (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    transaction_type ENUM('payment_in', 'cashout', 'wallet_credit', 'wallet_debit', 'commission', 'refund', 'adjustment') NOT NULL,
    amount DECIMAL(12,2) NOT NULL,
    currency VARCHAR(3) DEFAULT 'VND',
    
    -- Core references
    order_id BIGINT UNSIGNED NULL COMMENT 'Reference to orders table',
    customer_id BIGINT UNSIGNED NULL COMMENT 'Customer involved in transaction',
    vendor_id BIGINT UNSIGNED NULL COMMENT 'Vendor involved in transaction',
    
    -- Wallet related fields
    wallet_id BIGINT UNSIGNED NULL COMMENT 'Reference to wallets table',
    balance_before DECIMAL(12,2) NULL COMMENT 'Wallet balance before transaction',
    balance_after DECIMAL(12,2) NULL COMMENT 'Wallet balance after transaction',
    
    -- Status and metadata
    status ENUM('pending','completed','failed','cancelled') NOT NULL DEFAULT 'pending',
    description VARCHAR(255) NOT NULL COMMENT 'Human readable description',
    metadata JSON NULL COMMENT 'Additional transaction metadata',
    
    -- Reference to domain-specific tables (payments/cashouts will reference back to this)
    reference_type VARCHAR(50) NULL COMMENT 'Type of additional reference entity',
    reference_id BIGINT UNSIGNED NULL COMMENT 'ID of additional reference entity',
    
    -- Audit fields
    created_by BIGINT UNSIGNED NULL COMMENT 'User who initiated this transaction',
    processed_by BIGINT UNSIGNED NULL COMMENT 'User who processed this transaction',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE SET NULL,
    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE SET NULL,
    FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE SET NULL,
    FOREIGN KEY (wallet_id) REFERENCES wallets(id) ON DELETE SET NULL,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE SET NULL,
    FOREIGN KEY (processed_by) REFERENCES users(id) ON DELETE SET NULL,
    
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Central financial ledger - single source of truth for all money movements';

-- Payments table
CREATE TABLE payments (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    order_id BIGINT UNSIGNED NOT NULL,
    transaction_id BIGINT UNSIGNED NULL COMMENT 'Reference to transactions table for consistency',
    payment_method ENUM('credit_card', 'debit_card', 'paypal', 'stripe', 'bank_transfer', 'cod') NOT NULL,
    payment_gateway ENUM('stripe', 'paypal', 'vnpay', 'momo', 'manual') NOT NULL,
    gateway_transaction_id VARCHAR(255) UNIQUE COMMENT 'Transaction ID from payment gateway',
    amount DECIMAL(12,2) NOT NULL,
    status ENUM('pending', 'processing', 'completed', 'failed', 'refunded', 'partially_refunded') DEFAULT 'pending',
    gateway_response JSON COMMENT 'Raw response from payment gateway',
    refund_amount DECIMAL(12,2) DEFAULT 0.00,
    refund_reason VARCHAR(500),
    refunded_at TIMESTAMP NULL,
    paid_at TIMESTAMP NULL,
    failed_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE SET NULL,
    INDEX idx_order (order_id),
    INDEX idx_transaction (transaction_id),
    INDEX idx_gateway_transaction (gateway_transaction_id),
    INDEX idx_status (status),
    INDEX idx_payment_method (payment_method)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Payment transactions with gateway details';

-- Cashouts table (money going out - vendor payouts, expenses)
CREATE TABLE cashouts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    vendor_id BIGINT UNSIGNED NOT NULL,
    transaction_id BIGINT UNSIGNED NULL COMMENT 'Reference to transactions table for consistency',
    amount DECIMAL(12,2) NOT NULL,
    bank_code VARCHAR(20) NOT NULL,
    bank_account_number VARCHAR(50) NOT NULL,
    bank_account_holder VARCHAR(255) NOT NULL,
    status ENUM('pending','processing','completed','failed','cancelled') NOT NULL DEFAULT 'pending',
    cashout_type ENUM('commission_payout', 'vendor_payment', 'expense', 'refund') DEFAULT 'commission_payout',
    gateway_transaction_id VARCHAR(255) NULL COMMENT 'External payment gateway transaction ID',
    reference_type VARCHAR(50) NULL COMMENT 'Type of reference (order, request, etc.)',
    reference_id BIGINT UNSIGNED NULL COMMENT 'ID of reference entity',
    notes VARCHAR(500) NULL,
    processed_by BIGINT UNSIGNED NULL COMMENT 'Admin who processed this cashout',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMP NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (vendor_id) REFERENCES vendor_profiles(id) ON DELETE RESTRICT,
    FOREIGN KEY (transaction_id) REFERENCES transactions(id) ON DELETE SET NULL,
    FOREIGN KEY (bank_code) REFERENCES supported_banks(bank_code) ON DELETE RESTRICT,
    FOREIGN KEY (processed_by) REFERENCES users(id) ON DELETE SET NULL,
    UNIQUE KEY idx_unique_gateway_transaction (gateway_transaction_id),
    INDEX idx_vendor (vendor_id),
    INDEX idx_transaction (transaction_id),
    INDEX idx_status (status),
    INDEX idx_type (cashout_type),
    INDEX idx_processed (processed_at),
    INDEX idx_reference (reference_type, reference_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Money going out - vendor payouts and expenses with banking details';

-- Sales inventory tracking (stock out)
CREATE TABLE sales_inventory (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    order_id BIGINT UNSIGNED NULL COMMENT 'Order that caused this stock movement',
    quantity INT NOT NULL COMMENT 'Quantity sold (negative for returns)',
    unit_sale_price DECIMAL(12,2) NOT NULL COMMENT 'Sale price per unit',
    total_revenue DECIMAL(12,2) NOT NULL COMMENT 'Total revenue from this sale',
    commission_amount DECIMAL(12,2) DEFAULT 0.00 COMMENT 'Commission amount to be paid to vendor',
    balance_after INT NOT NULL COMMENT 'Stock balance after this sale',
    movement_type ENUM('sale', 'return', 'damage', 'loss', 'adjustment') DEFAULT 'sale',
    notes VARCHAR(500) NULL,
    created_by BIGINT UNSIGNED NOT NULL COMMENT 'User who recorded this movement',
    sold_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE SET NULL,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_product (product_id),
    INDEX idx_order (order_id),
    INDEX idx_movement_type (movement_type),
    INDEX idx_sold_date (sold_at),
    INDEX idx_created_by (created_by)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Sales inventory tracking - stock going out';

-- Reviews and ratings
CREATE TABLE product_reviews (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    order_id BIGINT UNSIGNED NOT NULL,
    customer_id BIGINT UNSIGNED NOT NULL,
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    title VARCHAR(255),
    comment TEXT,
    images VARCHAR(1000) COMMENT 'Review image URLs, comma-separated',
    helpful_count INT DEFAULT 0,
    unhelpful_count INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE CASCADE,
    UNIQUE KEY unique_product_order_customer (product_id, order_id, customer_id),
    INDEX idx_product_rating (product_id, rating),
    INDEX idx_customer (customer_id),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Product reviews and ratings';

-- =====================================================
-- ADDITIONAL INDEXES FOR PERFORMANCE OPTIMIZATION
-- =====================================================
CREATE INDEX idx_products_search ON products(is_active, category_id, price);
CREATE INDEX idx_orders_date_range ON orders(created_at, status);
CREATE INDEX idx_env_data_analysis ON environmental_data(farm_profile_id, measurement_date, soil_ph);
CREATE INDEX idx_weather_lookup ON weather_data_cache(farm_profile_id, weather_date, api_source);

-- Database schema completed
-- Version 5.0 - Major restructure: Removed vendor dependencies, added request system, restructured financial flow
-- Total tables: 31
-- Key changes:
-- - Products no longer tied to specific vendors (now in company inventory)
-- - Added commission tracking at product level
-- - Split inventory into purchase_inventory and sales_inventory
-- - Added comprehensive request management system
-- - Restructured financial system: payments (in) + cashouts (out) + transactions (history)
-- Ready for deployment
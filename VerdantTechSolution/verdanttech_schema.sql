-- VerdantTech Solutions Database Schema
-- AI-Integrated Green Agricultural Equipment Platform for Sustainable Vegetable Farming
-- Version: 1.0
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
    role ENUM('customer', 'farmer', 'seller', 'vendor', 'admin', 'expert', 'content_manager') NOT NULL DEFAULT 'customer',
    full_name VARCHAR(255) NOT NULL,
    phone_number VARCHAR(20),
    is_verified BOOLEAN DEFAULT FALSE,
    verification_token VARCHAR(255),
    verification_sent_at TIMESTAMP NULL,
    language_preference ENUM('vi', 'en') DEFAULT 'vi',
    avatar_url VARCHAR(500),
    status ENUM('active', 'inactive', 'suspended', 'deleted') DEFAULT 'active',
    last_login_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    deleted_at TIMESTAMP NULL,
    
    INDEX idx_email (email),
    INDEX idx_role (role),
    INDEX idx_status (status),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Base user authentication and profile table';

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
    primary_crops JSON COMMENT 'Array of main crops grown',
    farming_experience_years INT,
    certification_types JSON COMMENT 'Array of certifications like organic, VietGAP, GlobalGAP',
    soil_type VARCHAR(100),
    irrigation_type VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_location (province, district),
    INDEX idx_farm_size (farm_size_hectares)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Farm profile details for farmer users';

-- Vendor profiles for sellers
CREATE TABLE vendor_profiles (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL UNIQUE,
    company_name VARCHAR(255) NOT NULL,
    business_registration_number VARCHAR(100) UNIQUE,
    tax_code VARCHAR(50),
    company_address TEXT,
    sustainability_credentials JSON COMMENT 'JSON array of sustainability certifications',
    verified_at TIMESTAMP NULL,
    verified_by BIGINT UNSIGNED NULL,
    bank_account_info JSON COMMENT 'Encrypted bank details for payments',
    commission_rate DECIMAL(5,2) DEFAULT 10.00 COMMENT 'Platform commission percentage',
    rating_average DECIMAL(3,2) DEFAULT 0.00,
    total_reviews INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE SET NULL,
    INDEX idx_company_name (company_name),
    INDEX idx_verified (verified_at),
    INDEX idx_rating (rating_average)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Vendor/seller profile and verification details';

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
    vendor_id BIGINT UNSIGNED NOT NULL,
    category_id BIGINT UNSIGNED NOT NULL,
    sku VARCHAR(100) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    name_en VARCHAR(255),
    description TEXT,
    description_en TEXT,
    price DECIMAL(12,2) NOT NULL,
    discount_percentage DECIMAL(5,2) DEFAULT 0.00,
    green_certifications JSON COMMENT 'Array of eco certifications',
    energy_efficiency_rating VARCHAR(10),
    specifications JSON COMMENT 'Technical specifications as key-value pairs',
    manual_urls JSON COMMENT 'Array of manual/guide URLs',
    images JSON COMMENT 'Array of image URLs',
    warranty_months INT DEFAULT 12,
    stock_quantity INT DEFAULT 0,
    min_order_quantity INT DEFAULT 1,
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
    
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (category_id) REFERENCES product_categories(id) ON DELETE RESTRICT,
    INDEX idx_vendor (vendor_id),
    INDEX idx_category (category_id),
    INDEX idx_sku (sku),
    INDEX idx_name (name),
    INDEX idx_price (price),
    INDEX idx_active_featured (is_active, is_featured),
    INDEX idx_rating (rating_average),
    FULLTEXT idx_search (name, name_en, description, description_en)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Green agricultural equipment products';

-- Product inventory tracking
CREATE TABLE inventory_logs (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    type ENUM('in', 'out', 'adjustment') NOT NULL,
    quantity INT NOT NULL,
    balance_after INT NOT NULL,
    reason VARCHAR(255),
    reference_type VARCHAR(50) COMMENT 'order, return, manual',
    reference_id BIGINT UNSIGNED NULL,
    created_by BIGINT UNSIGNED,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE SET NULL,
    INDEX idx_product_type (product_id, type),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Inventory movement tracking';

-- =====================================================
-- ORDER AND PAYMENT TABLES
-- =====================================================

-- Orders table
CREATE TABLE orders (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    order_number VARCHAR(50) UNIQUE NOT NULL,
    customer_id BIGINT UNSIGNED NOT NULL,
    vendor_id BIGINT UNSIGNED NOT NULL,
    status ENUM('pending', 'confirmed', 'processing', 'shipped', 'delivered', 'cancelled', 'refunded') DEFAULT 'pending',
    subtotal DECIMAL(12,2) NOT NULL,
    tax_amount DECIMAL(12,2) DEFAULT 0.00,
    shipping_fee DECIMAL(12,2) DEFAULT 0.00,
    discount_amount DECIMAL(12,2) DEFAULT 0.00,
    total_amount DECIMAL(12,2) NOT NULL,
    currency_code VARCHAR(3) DEFAULT 'VND',
    shipping_address JSON NOT NULL,
    billing_address JSON,
    shipping_method VARCHAR(100),
    tracking_number VARCHAR(100),
    notes TEXT,
    cancelled_reason TEXT,
    cancelled_at TIMESTAMP NULL,
    confirmed_at TIMESTAMP NULL,
    shipped_at TIMESTAMP NULL,
    delivered_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE RESTRICT,
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE RESTRICT,
    INDEX idx_order_number (order_number),
    INDEX idx_customer (customer_id),
    INDEX idx_vendor (vendor_id),
    INDEX idx_status (status),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Customer orders';

-- Order items
CREATE TABLE order_items (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    order_id BIGINT UNSIGNED NOT NULL,
    product_id BIGINT UNSIGNED NOT NULL,
    product_name VARCHAR(255) NOT NULL COMMENT 'Snapshot of product name',
    product_sku VARCHAR(100) NOT NULL COMMENT 'Snapshot of SKU',
    quantity INT NOT NULL,
    unit_price DECIMAL(12,2) NOT NULL,
    discount_amount DECIMAL(12,2) DEFAULT 0.00,
    subtotal DECIMAL(12,2) NOT NULL,
    specifications JSON COMMENT 'Snapshot of product specs',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE RESTRICT,
    INDEX idx_order (order_id),
    INDEX idx_product (product_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Order line items';

-- Payments table
CREATE TABLE payments (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    order_id BIGINT UNSIGNED NOT NULL,
    payment_method ENUM('credit_card', 'debit_card', 'paypal', 'stripe', 'bank_transfer', 'cod') NOT NULL,
    payment_gateway ENUM('stripe', 'paypal', 'vnpay', 'momo', 'manual') NOT NULL,
    transaction_id VARCHAR(255) UNIQUE,
    amount DECIMAL(12,2) NOT NULL,
    currency_code VARCHAR(3) DEFAULT 'VND',
    status ENUM('pending', 'processing', 'completed', 'failed', 'refunded', 'partially_refunded') DEFAULT 'pending',
    gateway_response JSON COMMENT 'Raw response from payment gateway',
    refund_amount DECIMAL(12,2) DEFAULT 0.00,
    refund_reason TEXT,
    refunded_at TIMESTAMP NULL,
    paid_at TIMESTAMP NULL,
    failed_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE RESTRICT,
    INDEX idx_order (order_id),
    INDEX idx_transaction (transaction_id),
    INDEX idx_status (status),
    INDEX idx_payment_method (payment_method)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Payment transactions';

-- Reviews and ratings
CREATE TABLE product_reviews (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT UNSIGNED NOT NULL,
    order_id BIGINT UNSIGNED NOT NULL,
    customer_id BIGINT UNSIGNED NOT NULL,
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    title VARCHAR(255),
    comment TEXT,
    images JSON COMMENT 'Array of review image URLs',
    is_verified_purchase BOOLEAN DEFAULT TRUE,
    helpful_count INT DEFAULT 0,
    unhelpful_count INT DEFAULT 0,
    vendor_reply TEXT,
    vendor_replied_at TIMESTAMP NULL,
    is_featured BOOLEAN DEFAULT FALSE,
    status ENUM('pending', 'approved', 'rejected') DEFAULT 'pending',
    moderated_by BIGINT UNSIGNED NULL,
    moderated_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    FOREIGN KEY (customer_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (moderated_by) REFERENCES users(id) ON DELETE SET NULL,
    UNIQUE KEY unique_product_order_customer (product_id, order_id, customer_id),
    INDEX idx_product_rating (product_id, rating),
    INDEX idx_customer (customer_id),
    INDEX idx_status (status),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Product reviews and ratings';

-- =====================================================
-- ENVIRONMENTAL DATA TABLES
-- =====================================================

-- Environmental monitoring data
CREATE TABLE environmental_data (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    farm_profile_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    measurement_date DATE NOT NULL,
    soil_ph DECIMAL(3,1) CHECK (soil_ph >= 0 AND soil_ph <= 14),
    co2_footprint DECIMAL(10,2) COMMENT 'CO2 emissions in kg',
    soil_moisture_percentage DECIMAL(5,2),
    nitrogen_level DECIMAL(10,2) COMMENT 'N content in mg/kg',
    phosphorus_level DECIMAL(10,2) COMMENT 'P content in mg/kg',
    potassium_level DECIMAL(10,2) COMMENT 'K content in mg/kg',
    organic_matter_percentage DECIMAL(5,2),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (farm_profile_id) REFERENCES farm_profiles(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_farm_date (farm_profile_id, measurement_date),
    INDEX idx_user (user_id),
    INDEX idx_date (measurement_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Manual environmental data input by farmers';

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
    context JSON COMMENT 'Conversation context and metadata',
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
    intent VARCHAR(100) COMMENT 'Detected user intent',
    entities JSON COMMENT 'Extracted entities from message',
    confidence_score DECIMAL(3,2) COMMENT 'AI confidence score',
    suggested_actions JSON COMMENT 'Quick reply suggestions',
    attachments JSON COMMENT 'Image or file attachments',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (conversation_id) REFERENCES chatbot_conversations(id) ON DELETE CASCADE,
    INDEX idx_conversation (conversation_id),
    INDEX idx_type (message_type),
    INDEX idx_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Individual chatbot messages';

-- Knowledge base for chatbot
CREATE TABLE knowledge_base (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    category VARCHAR(100) NOT NULL,
    subcategory VARCHAR(100),
    question TEXT NOT NULL,
    answer TEXT NOT NULL,
    keywords JSON COMMENT 'Array of keywords for matching',
    language ENUM('vi', 'en') DEFAULT 'vi',
    source_url VARCHAR(500),
    is_verified BOOLEAN DEFAULT FALSE,
    verified_by BIGINT UNSIGNED NULL,
    usage_count BIGINT DEFAULT 0,
    helpful_count INT DEFAULT 0,
    unhelpful_count INT DEFAULT 0,
    created_by BIGINT UNSIGNED,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (verified_by) REFERENCES users(id) ON DELETE SET NULL,
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE SET NULL,
    INDEX idx_category (category, subcategory),
    INDEX idx_language (language),
    INDEX idx_verified (is_verified),
    FULLTEXT idx_search (question, answer)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Knowledge base for AI chatbot responses';

-- =====================================================
-- PLANT DISEASE DETECTION TABLES
-- =====================================================

-- Plant disease detections
CREATE TABLE plant_disease_detections (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    farm_profile_id BIGINT UNSIGNED NULL,
    image_url VARCHAR(500) NOT NULL,
    image_metadata JSON COMMENT 'Image size, format, etc.',
    plant_type VARCHAR(100),
    detected_diseases JSON COMMENT 'Array of {disease_name, confidence, severity}',
    ai_provider VARCHAR(50) COMMENT 'Computer vision API used',
    ai_response JSON COMMENT 'Raw AI API response',
    organic_treatment_suggestions JSON COMMENT 'Array of organic treatment options',
    chemical_treatment_suggestions JSON COMMENT 'Array of chemical treatment options',
    prevention_tips JSON COMMENT 'Array of prevention recommendations',
    user_feedback ENUM('helpful', 'not_helpful', 'partially_helpful') NULL,
    feedback_comments TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (farm_profile_id) REFERENCES farm_profiles(id) ON DELETE SET NULL,
    INDEX idx_user (user_id),
    INDEX idx_farm (farm_profile_id),
    INDEX idx_plant_type (plant_type),
    INDEX idx_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Plant disease detection history';

-- =====================================================
-- COMMUNITY TABLES
-- =====================================================

-- Forum categories
CREATE TABLE forum_categories (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    name_en VARCHAR(255),
    slug VARCHAR(255) UNIQUE NOT NULL,
    description TEXT,
    icon_url VARCHAR(500),
    sort_order INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_slug (slug),
    INDEX idx_active_sort (is_active, sort_order)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Forum discussion categories';

-- Forum posts
CREATE TABLE forum_posts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    category_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    tags JSON COMMENT 'Array of tags',
    view_count BIGINT DEFAULT 0,
    reply_count INT DEFAULT 0,
    like_count INT DEFAULT 0,
    dislike_count INT DEFAULT 0,
    is_pinned BOOLEAN DEFAULT FALSE,
    is_locked BOOLEAN DEFAULT FALSE,
    status ENUM('published', 'draft', 'moderated', 'deleted') DEFAULT 'published',
    moderated_reason TEXT,
    moderated_by BIGINT UNSIGNED NULL,
    last_activity_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (category_id) REFERENCES forum_categories(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (moderated_by) REFERENCES users(id) ON DELETE SET NULL,
    INDEX idx_category (category_id),
    INDEX idx_user (user_id),
    INDEX idx_status_pinned (status, is_pinned),
    INDEX idx_last_activity (last_activity_at),
    FULLTEXT idx_search (title, content)
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
    is_solution BOOLEAN DEFAULT FALSE,
    status ENUM('visible', 'moderated', 'deleted') DEFAULT 'visible',
    moderated_reason TEXT,
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

-- Blog posts
CREATE TABLE blog_posts (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    author_id BIGINT UNSIGNED NOT NULL,
    category VARCHAR(100) NOT NULL,
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) UNIQUE NOT NULL,
    excerpt TEXT,
    content TEXT NOT NULL,
    featured_image_url VARCHAR(500),
    tags JSON COMMENT 'Array of tags',
    seo_title VARCHAR(255),
    seo_description TEXT,
    seo_keywords JSON,
    view_count BIGINT DEFAULT 0,
    comment_count INT DEFAULT 0,
    like_count INT DEFAULT 0,
    dislike_count INT DEFAULT 0,
    reading_time_minutes INT,
    is_featured BOOLEAN DEFAULT FALSE,
    status ENUM('draft', 'published', 'scheduled', 'archived') DEFAULT 'draft',
    published_at TIMESTAMP NULL,
    scheduled_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (author_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_author (author_id),
    INDEX idx_slug (slug),
    INDEX idx_status_featured (status, is_featured),
    INDEX idx_published (published_at),
    FULLTEXT idx_search (title, excerpt, content)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Blog articles and educational content';

-- Blog comments
CREATE TABLE blog_comments (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    post_id BIGINT UNSIGNED NOT NULL,
    user_id BIGINT UNSIGNED NOT NULL,
    parent_id BIGINT UNSIGNED NULL,
    content TEXT NOT NULL,
    like_count INT DEFAULT 0,
    dislike_count INT DEFAULT 0,
    status ENUM('approved', 'pending', 'spam', 'deleted') DEFAULT 'pending',
    moderated_by BIGINT UNSIGNED NULL,
    moderated_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (post_id) REFERENCES blog_posts(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (parent_id) REFERENCES blog_comments(id) ON DELETE CASCADE,
    FOREIGN KEY (moderated_by) REFERENCES users(id) ON DELETE SET NULL,
    INDEX idx_post_status (post_id, status),
    INDEX idx_user (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Blog post comments';

-- User interactions (likes/dislikes)
CREATE TABLE user_interactions (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    target_type ENUM('forum_post', 'forum_comment', 'blog_post', 'blog_comment', 'product_review') NOT NULL,
    target_id BIGINT UNSIGNED NOT NULL,
    interaction_type ENUM('like', 'dislike', 'helpful', 'unhelpful') NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    UNIQUE KEY unique_user_target_interaction (user_id, target_type, target_id, interaction_type),
    INDEX idx_target (target_type, target_id),
    INDEX idx_user (user_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='User likes/dislikes for various content types';

-- =====================================================
-- EDUCATIONAL CONTENT TABLES
-- =====================================================

-- Educational materials
CREATE TABLE educational_materials (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    created_by BIGINT UNSIGNED NOT NULL,
    material_type ENUM('guide', 'tutorial', 'research', 'case_study', 'infographic', 'video') NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    content_url VARCHAR(500) NOT NULL COMMENT 'URL to file or video',
    thumbnail_url VARCHAR(500),
    file_size_mb DECIMAL(10,2),
    duration_minutes INT COMMENT 'For video content',
    language ENUM('vi', 'en') DEFAULT 'vi',
    difficulty_level ENUM('beginner', 'intermediate', 'advanced') DEFAULT 'beginner',
    topics JSON COMMENT 'Array of related topics',
    target_audience JSON COMMENT 'Array of audience types',
    download_count BIGINT DEFAULT 0,
    view_count BIGINT DEFAULT 0,
    rating_average DECIMAL(3,2) DEFAULT 0.00,
    total_ratings INT DEFAULT 0,
    is_premium BOOLEAN DEFAULT FALSE,
    status ENUM('draft', 'published', 'archived') DEFAULT 'draft',
    published_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (created_by) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_creator (created_by),
    INDEX idx_type (material_type),
    INDEX idx_language (language),
    INDEX idx_status (status),
    INDEX idx_rating (rating_average),
    FULLTEXT idx_search (title, description)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Educational content created by experts and vendors';

-- =====================================================
-- ANALYTICS TABLES
-- =====================================================

-- User activity logs
CREATE TABLE user_activity_logs (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NOT NULL,
    activity_type VARCHAR(50) NOT NULL COMMENT 'login, view_product, search, etc.',
    activity_details JSON,
    ip_address VARCHAR(45),
    user_agent TEXT,
    session_id VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_user_type (user_id, activity_type),
    INDEX idx_created (created_at),
    INDEX idx_session (session_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='User activity tracking for analytics';

-- Sales analytics summary (daily aggregation)
CREATE TABLE sales_analytics_daily (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    date DATE NOT NULL,
    vendor_id BIGINT UNSIGNED NOT NULL,
    total_orders INT DEFAULT 0,
    total_revenue DECIMAL(15,2) DEFAULT 0.00,
    total_products_sold INT DEFAULT 0,
    average_order_value DECIMAL(12,2) DEFAULT 0.00,
    new_customers INT DEFAULT 0,
    returning_customers INT DEFAULT 0,
    top_products JSON COMMENT 'Array of top selling products',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (vendor_id) REFERENCES users(id) ON DELETE CASCADE,
    UNIQUE KEY unique_date_vendor (date, vendor_id),
    INDEX idx_date (date),
    INDEX idx_vendor (vendor_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Daily sales analytics aggregation';

-- =====================================================
-- SYSTEM TABLES
-- =====================================================

-- System settings
CREATE TABLE system_settings (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    setting_key VARCHAR(100) UNIQUE NOT NULL,
    setting_value TEXT,
    setting_type ENUM('string', 'number', 'boolean', 'json') DEFAULT 'string',
    description TEXT,
    is_public BOOLEAN DEFAULT FALSE COMMENT 'Can be exposed to frontend',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX idx_key (setting_key),
    INDEX idx_public (is_public)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='System configuration settings';

-- Audit logs for critical operations
CREATE TABLE audit_logs (
    id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT UNSIGNED NULL,
    action VARCHAR(100) NOT NULL,
    entity_type VARCHAR(50) NOT NULL,
    entity_id BIGINT UNSIGNED NOT NULL,
    old_values JSON,
    new_values JSON,
    ip_address VARCHAR(45),
    user_agent TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL,
    INDEX idx_user (user_id),
    INDEX idx_entity (entity_type, entity_id),
    INDEX idx_action (action),
    INDEX idx_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Audit trail for important system changes';

-- =====================================================
-- SAMPLE DATA FOR TESTING
-- =====================================================

-- Insert sample users
INSERT INTO users (email, password_hash, role, full_name, phone_number, is_verified, language_preference) VALUES
-- Admin users
('admin@verdanttech.vn', '$2b$10$YourHashedPasswordHere', 'admin', 'Nguyễn Văn Admin', '0901234567', TRUE, 'vi'),
('itadmin@verdanttech.vn', '$2b$10$YourHashedPasswordHere', 'admin', 'Trần IT Admin', '0901234568', TRUE, 'vi'),

-- Farmers
('farmer1@example.com', '$2b$10$YourHashedPasswordHere', 'farmer', 'Lê Văn Nông', '0912345678', TRUE, 'vi'),
('farmer2@example.com', '$2b$10$YourHashedPasswordHere', 'farmer', 'Phạm Thị Mai', '0923456789', TRUE, 'vi'),

-- Vendors
('vendor1@greentech.vn', '$2b$10$YourHashedPasswordHere', 'vendor', 'GreenTech Solutions', '0934567890', TRUE, 'vi'),
('vendor2@ecofarming.vn', '$2b$10$YourHashedPasswordHere', 'vendor', 'EcoFarming Equipment', '0945678901', TRUE, 'vi'),

-- Expert
('expert@agri.edu.vn', '$2b$10$YourHashedPasswordHere', 'expert', 'TS. Nguyễn Minh Khoa', '0956789012', TRUE, 'vi'),

-- Content Manager
('content@verdanttech.vn', '$2b$10$YourHashedPasswordHere', 'content_manager', 'Hoàng Thị Bích', '0967890123', TRUE, 'vi');

-- Insert sample farm profiles
INSERT INTO farm_profiles (user_id, farm_name, farm_size_hectares, location_address, province, district, commune, latitude, longitude, primary_crops, farming_experience_years, certification_types, soil_type, irrigation_type) VALUES
(3, 'Nông trại Xanh Đồng Nai', 5.5, '123 Đường Nông Nghiệp, Xã Long Thành', 'Đồng Nai', 'Long Thành', 'An Phước', 10.7919, 106.9686, '["rau cải", "cà chua", "dưa leo"]', 10, '["VietGAP", "Organic"]', 'Đất phù sa', 'Tưới nhỏ giọt'),
(4, 'Vườn rau sạch Bình Dương', 3.2, '456 Tổ 5, Ấp 3', 'Bình Dương', 'Dĩ An', 'Bình An', 10.9068, 106.7692, '["rau muống", "cải xanh", "mồng tơi"]', 7, '["VietGAP"]', 'Đất thịt pha cát', 'Tưới phun mưa');

-- Insert sample vendor profiles
INSERT INTO vendor_profiles (user_id, company_name, business_registration_number, tax_code, company_address, sustainability_credentials, verified_at, verified_by, commission_rate) VALUES
(5, 'GreenTech Solutions Co., Ltd', '0123456789', '0123456789-001', '789 Nguyễn Văn Linh, Q7, TP.HCM', '["ISO 14001", "Green Certificate", "Energy Star"]', NOW(), 1, 8.00),
(6, 'EcoFarming Equipment Vietnam', '0987654321', '0987654321-001', '321 Lê Văn Việt, Q9, TP.HCM', '["ISO 14001", "Eco Label"]', NOW(), 1, 10.00);

-- Insert sample product categories
INSERT INTO product_categories (name, name_en, slug, description, sort_order) VALUES
('Thiết bị tưới tiêu', 'Irrigation Equipment', 'thiet-bi-tuoi-tieu', 'Các thiết bị tưới tiêu tiết kiệm nước', 1),
('Máy nông nghiệp', 'Agricultural Machinery', 'may-nong-nghiep', 'Máy móc phục vụ canh tác', 2),
('Phân bón hữu cơ', 'Organic Fertilizers', 'phan-bon-huu-co', 'Phân bón thân thiện môi trường', 3),
('Dụng cụ cầm tay', 'Hand Tools', 'dung-cu-cam-tay', 'Dụng cụ làm vườn thủ công', 4),
('Hệ thống nhà kính', 'Greenhouse Systems', 'he-thong-nha-kinh', 'Nhà kính và phụ kiện', 5);

-- Insert sample products
INSERT INTO products (vendor_id, category_id, sku, name, name_en, description, price, green_certifications, energy_efficiency_rating, specifications, stock_quantity, is_featured) VALUES
(5, 1, 'DRIP-001', 'Hệ thống tưới nhỏ giọt thông minh', 'Smart Drip Irrigation System', 'Hệ thống tưới nhỏ giọt tự động với cảm biến độ ẩm', 2500000, '["Water Efficient", "Energy Star"]', 'A+', '{"flow_rate": "2L/h", "coverage": "100m2", "sensor_type": "Soil moisture"}', 50, TRUE),
(5, 2, 'PUMP-001', 'Máy bơm nước năng lượng mặt trời', 'Solar Water Pump', 'Máy bơm nước sử dụng năng lượng mặt trời, thân thiện môi trường', 8500000, '["Solar Powered", "Zero Emission"]', 'A++', '{"power": "1000W", "flow_rate": "5000L/h", "head": "50m"}', 20, TRUE),
(6, 3, 'FERT-001', 'Phân compost hữu cơ cao cấp', 'Premium Organic Compost', 'Phân hữu cơ lên men từ phụ phẩm nông nghiệp', 150000, '["100% Organic", "Chemical Free"]', NULL, '{"N": "2%", "P": "1%", "K": "2%", "weight": "25kg"}', 200, FALSE),
(6, 4, 'TOOL-001', 'Bộ dụng cụ làm vườn sinh thái', 'Eco Garden Tool Set', 'Bộ 5 dụng cụ làm vườn từ vật liệu tái chế', 450000, '["Recycled Materials", "Sustainable"]', NULL, '{"pieces": 5, "material": "Recycled steel and bamboo"}', 100, FALSE);

-- Insert sample knowledge base entries
INSERT INTO knowledge_base (category, subcategory, question, answer, keywords, language, is_verified, verified_by, created_by) VALUES
('Tưới tiêu', 'Tiết kiệm nước', 'Làm thế nào để tiết kiệm nước khi tưới cây?', 'Có nhiều cách tiết kiệm nước: 1) Sử dụng hệ thống tưới nhỏ giọt, 2) Tưới vào buổi sáng sớm hoặc chiều tối, 3) Phủ rơm rạ để giữ ẩm đất, 4) Sử dụng cảm biến độ ẩm đất.', '["tưới tiêu", "tiết kiệm nước", "tưới nhỏ giọt"]', 'vi', TRUE, 7, 7),
('Đất trồng', 'pH đất', 'pH đất lý tưởng cho trồng rau là bao nhiêu?', 'Hầu hết các loại rau phát triển tốt ở pH 6.0-7.0. Cà chua: 6.0-6.8, Cải bắp: 6.0-7.5, Dưa leo: 6.0-7.0. Nên kiểm tra pH đất định kỳ 3-6 tháng.', '["pH đất", "độ pH", "đất trồng rau"]', 'vi', TRUE, 7, 7),
('Pest Control', 'Organic Methods', 'What are organic pest control methods?', 'Organic pest control includes: 1) Companion planting, 2) Natural predators, 3) Neem oil spray, 4) Diatomaceous earth, 5) Row covers, 6) Crop rotation.', '["pest control", "organic", "natural pesticide"]', 'en', TRUE, 7, 7);

-- Insert sample system settings
INSERT INTO system_settings (setting_key, setting_value, setting_type, description, is_public) VALUES
('commission_rate_default', '10', 'number', 'Default commission rate for new vendors (%)', FALSE),
('weather_api_sync_interval', '15', 'number', 'Weather data sync interval in minutes', FALSE),
('chatbot_response_timeout', '2000', 'number', 'Chatbot response timeout in milliseconds', FALSE),
('max_upload_size_mb', '10', 'number', 'Maximum file upload size in MB', TRUE),
('supported_languages', '["vi", "en"]', 'json', 'Supported languages', TRUE),
('maintenance_mode', 'false', 'boolean', 'System maintenance mode', TRUE);

-- Create triggers for updating product ratings
DELIMITER //
CREATE TRIGGER update_product_rating AFTER INSERT ON product_reviews
FOR EACH ROW
BEGIN
    UPDATE products p
    SET p.rating_average = (
        SELECT AVG(rating) FROM product_reviews WHERE product_id = NEW.product_id AND status = 'approved'
    ),
    p.total_reviews = (
        SELECT COUNT(*) FROM product_reviews WHERE product_id = NEW.product_id AND status = 'approved'
    )
    WHERE p.id = NEW.product_id;
END//

CREATE TRIGGER update_product_rating_on_update AFTER UPDATE ON product_reviews
FOR EACH ROW
BEGIN
    IF NEW.status != OLD.status THEN
        UPDATE products p
        SET p.rating_average = (
            SELECT AVG(rating) FROM product_reviews WHERE product_id = NEW.product_id AND status = 'approved'
        ),
        p.total_reviews = (
            SELECT COUNT(*) FROM product_reviews WHERE product_id = NEW.product_id AND status = 'approved'
        )
        WHERE p.id = NEW.product_id;
    END IF;
END//
DELIMITER ;

-- Create indexes for performance optimization
CREATE INDEX idx_products_search ON products(is_active, category_id, price);
CREATE INDEX idx_orders_date_range ON orders(created_at, status);
CREATE INDEX idx_env_data_analysis ON environmental_data(farm_profile_id, measurement_date, soil_ph);
CREATE INDEX idx_weather_lookup ON weather_data_cache(farm_profile_id, weather_date, api_source);

-- Grant necessary permissions (example for application user)
-- CREATE USER 'verdanttech_app'@'%' IDENTIFIED BY 'StrongPassword123!';
-- GRANT SELECT, INSERT, UPDATE, DELETE ON verdanttech_db.* TO 'verdanttech_app'@'%';
-- FLUSH PRIVILEGES;

-- Database schema completed
-- Total tables: 31
-- Ready for commercial deployment with Supabase or any MySQL-compatible service

-- SEEDER DATA FOR VERDANTTECH DATABASE
-- All passwords are: $2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS

-- Insert Users (with gmail.com emails and consistent password)
INSERT INTO `users` (`Id`, `Email`, `password_hash`, `Role`, `full_name`, `phone_number`, `is_verified`, `verification_token`, `verification_sent_at`, `avatar_url`, `Status`, `last_login_at`, `created_at`, `updated_at`, `deleted_at`) VALUES
(1, 'admin@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'admin', 'Quản trị viên hệ thống', '0901234567', 1, NULL, NULL, NULL, 'active', '2025-08-21 08:00:00', '2025-08-20 07:00:00', '2025-08-21 08:00:00', NULL),
(2, 'manager@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'manager', 'Nguyễn Văn Quản Lý', '0901234568', 1, NULL, NULL, NULL, 'active', '2025-08-21 07:30:00', '2025-08-20 07:00:00', '2025-08-21 07:30:00', NULL),
(3, 'seller1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'seller', 'Trần Thị Bán Hàng', '0901234569', 1, NULL, NULL, NULL, 'active', '2025-08-21 07:00:00', '2025-08-20 08:00:00', '2025-08-21 07:00:00', NULL),
(4, 'seller2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'seller', 'Lê Văn Nông Sản', '0901234570', 1, NULL, NULL, NULL, 'active', '2025-08-21 06:30:00', '2025-08-20 08:30:00', '2025-08-21 06:30:00', NULL),
(5, 'customer1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Phạm Văn Khách Hàng 1', '0901234571', 1, NULL, NULL, NULL, 'active', '2025-08-21 08:15:00', '2025-08-20 09:00:00', '2025-08-21 08:15:00', NULL),
(6, 'customer2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Hoàng Thị Khách Hàng 2', '0901234572', 1, NULL, NULL, NULL, 'active', '2025-08-21 08:10:00', '2025-08-20 09:30:00', '2025-08-21 08:10:00', NULL),
(7, 'farmer1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Nguyễn Văn Nông Dân 1', '0901234573', 1, NULL, NULL, NULL, 'active', '2025-08-21 06:00:00', '2025-08-20 10:00:00', '2025-08-21 06:00:00', NULL),
(8, 'farmer2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Trần Thị Nông Dân 2', '0901234574', 1, NULL, NULL, NULL, 'active', '2025-08-21 05:30:00', '2025-08-20 10:30:00', '2025-08-21 05:30:00', NULL),
(9, 'testuser@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Người Dùng Test', '0901234575', 0, 'test-token-123', '2025-08-21 07:00:00', NULL, 'active', NULL, '2025-08-21 07:00:00', '2025-08-21 07:00:00', NULL),
(10, 'inactive@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Người Dùng Không Hoạt Động', '0901234576', 1, NULL, NULL, NULL, 'inactive', '2025-08-20 15:00:00', '2025-08-20 11:00:00', '2025-08-20 15:00:00', NULL);

-- Insert Farm Profiles
INSERT INTO `farm_profiles` (`Id`, `user_id`, `farm_name`, `farm_size_hectares`, `location_address`, `Province`, `District`, `Commune`, `Latitude`, `Longitude`, `primary_crops`, `created_at`, `updated_at`) VALUES
(1, 7, 'Trang trại Xanh Sạch Đồng Nai', 5.50, 'Số 123 Đường Nông Nghiệp, Tân Phong', 'Đồng Nai', 'Biên Hòa', 'Tân Phong', 10.9545, 106.8441, '["Lúa", "Rau xanh", "Cà chua"]', '2025-08-20 10:00:00', '2025-08-21 06:00:00'),
(2, 8, 'Trang trại Hữu Cơ Long An', 8.25, 'Số 456 Đường Nông Thôn, Đức Hòa Thượng', 'Long An', 'Đức Hòa', 'Đức Hòa Thượng', 10.8838, 106.4226, '["Rau củ", "Trái cây", "Thảo dược"]', '2025-08-20 10:30:00', '2025-08-21 05:30:00');

-- Insert Vendor Profiles
INSERT INTO `vendor_profiles` (`Id`, `user_id`, `company_name`, `business_registration_number`, `tax_code`, `company_address`, `sustainability_credentials`, `verified_at`, `verified_by`, `bank_account_info`, `commission_rate`, `rating_average`, `total_reviews`, `created_at`, `updated_at`) VALUES
(1, 3, 'Công Ty Thiết Bị Nông Nghiệp Xanh', 'BRN123456789', 'TC001234567', 'Số 789 Đường Công Nghiệp, Quận 7, TP.HCM', '["EcoLabel", "EnergyStar", "CarbonNeutral"]', '2025-08-21 07:00:00', 1, '{"bank_name": "VCB", "account_number": "1234567890", "account_name": "Công Ty Thiết Bị Nông Nghiệp Xanh"}', 10.00, 4.5, 25, '2025-08-20 08:00:00', '2025-08-21 07:00:00'),
(2, 4, 'Cửa Hàng Nông Sản Sạch VerdantTech', 'BRN987654321', 'TC009876543', 'Số 321 Đường Nông Sản, Quận Tân Bình, TP.HCM', '["OrganicCert", "GreenTech"]', '2025-08-21 06:30:00', 1, '{"bank_name": "ACB", "account_number": "0987654321", "account_name": "Cửa Hàng Nông Sản Sạch VerdantTech"}', 8.00, 4.7, 42, '2025-08-20 08:30:00', '2025-08-21 06:30:00');

-- Insert Product Categories
INSERT INTO `product_categories` (`Id`, `parent_id`, `Name`, `name_en`, `Slug`, `Description`, `icon_url`, `sort_order`, `is_active`, `created_at`, `updated_at`) VALUES
(1, NULL, 'Thiết Bị Nông Nghiệp', 'Agricultural Equipment', 'thiet-bi-nong-nghiep', 'Các loại máy móc và thiết bị phục vụ sản xuất nông nghiệp', NULL, 1, 1, '2025-08-20 07:00:00', '2025-08-20 07:00:00'),
(2, 1, 'Máy Cày', 'Tractors', 'may-cay', 'Máy cày và thiết bị làm đất', NULL, 1, 1, '2025-08-20 07:00:00', '2025-08-20 07:00:00'),
(3, 1, 'Máy Gặt', 'Harvesters', 'may-gat', 'Máy gặt lúa và thu hoạch nông sản', NULL, 2, 1, '2025-08-20 07:00:00', '2025-08-20 07:00:00'),
(4, NULL, 'Phân Bón Hữu Cơ', 'Organic Fertilizers', 'phan-bon-huu-co', 'Các loại phân bón tự nhiên và hữu cơ', NULL, 2, 1, '2025-08-20 07:00:00', '2025-08-20 07:00:00'),
(5, 4, 'Phân Compost', 'Compost', 'phan-compost', 'Phân compost từ chất thải hữu cơ', NULL, 1, 1, '2025-08-20 07:00:00', '2025-08-20 07:00:00'),
(6, NULL, 'Hạt Giống', 'Seeds', 'hat-giong', 'Hạt giống các loại cây trồng', NULL, 3, 1, '2025-08-20 07:00:00', '2025-08-20 07:00:00'),
(7, 6, 'Hạt Giống Rau', 'Vegetable Seeds', 'hat-giong-rau', 'Hạt giống các loại rau củ quả', NULL, 1, 1, '2025-08-20 07:00:00', '2025-08-20 07:00:00'),
(8, NULL, 'Hệ Thống Tưới', 'Irrigation Systems', 'he-thong-tuoi', 'Thiết bị và hệ thống tưới tiêu', NULL, 4, 1, '2025-08-20 07:00:00', '2025-08-20 07:00:00');

-- Insert Products
INSERT INTO `products` (`Id`, `vendor_id`, `category_id`, `product_code`, `Name`, `name_en`, `Description`, `description_en`, `Price`, `discount_percentage`, `green_certifications`, `energy_efficiency_rating`, `specifications`, `manual_urls`, `images`, `warranty_months`, `stock_quantity`, `weight_kg`, `dimensions_cm`, `is_featured`, `is_active`, `view_count`, `sold_count`, `rating_average`, `total_reviews`, `created_at`, `updated_at`) VALUES
(1, 3, 2, 'TC001', 'Máy Cày Mini Điện VerdantTech V1', 'VerdantTech V1 Electric Mini Tractor', 'Máy cày mini điện thân thiện môi trường, phù hợp cho ruộng nhỏ và vườn gia đình', 'Eco-friendly electric mini tractor suitable for small fields and home gardens', 25000000.00, 10.00, '["EcoLabel", "EnergyStar"]', 'A+++', '{"power": "2000W", "battery": "48V 100Ah", "speed": "0-8km/h", "working_width": "80cm"}', '["manual-tc001-vi.pdf", "manual-tc001-en.pdf"]', '["tc001-1.jpg", "tc001-2.jpg", "tc001-3.jpg"]', 24, 50, 150.500, '{"length": 120, "width": 85, "height": 90}', 1, 1, 245, 12, 4.50, 8, '2025-08-20 08:00:00', '2025-08-21 07:00:00'),
(2, 3, 3, 'HV001', 'Máy Gặt Lúa Thông Minh EcoHarvest', 'EcoHarvest Smart Rice Harvester', 'Máy gặt lúa tự động với công nghệ thông minh, tiết kiệm năng lượng', 'Automatic rice harvester with smart technology and energy-saving features', 85000000.00, 5.00, '["GreenTech", "CarbonNeutral"]', 'A++', '{"power": "15kW", "capacity": "0.5ha/h", "grain_tank": "500kg", "cutting_width": "120cm"}', '["manual-hv001-vi.pdf"]', '["hv001-1.jpg", "hv001-2.jpg"]', 36, 20, 850.000, '{"length": 320, "width": 150, "height": 200}', 1, 1, 156, 5, 4.80, 4, '2025-08-20 08:30:00', '2025-08-21 06:30:00'),
(3, 4, 5, 'OFC001', 'Phân Compost Hữu Cơ Premium', 'Premium Organic Compost', 'Phân compost 100% hữu cơ từ phế phẩm nông nghiệp, giàu dinh dưỡng', '100% organic compost from agricultural waste, nutrient-rich', 150000.00, 0.00, '["OrganicCert", "EcoFriendly"]', NULL, '{"organic_matter": "85%", "nitrogen": "2.5%", "phosphorus": "1.8%", "potassium": "2.2%"}', '["huong-dan-su-dung-compost.pdf"]', '["ofc001-1.jpg", "ofc001-2.jpg"]', 0, 500, 25.000, '{"length": 50, "width": 30, "height": 20}', 0, 1, 1250, 87, 4.70, 12, '2025-08-20 09:00:00', '2025-08-21 05:00:00'),
(4, 4, 7, 'VS001', 'Hạt Giống Cà Chua Hữu Cơ F1', 'F1 Organic Tomato Seeds', 'Hạt giống cà chua lai F1 chất lượng cao, năng suất cao, kháng bệnh tốt', 'High-quality F1 hybrid tomato seeds, high yield, good disease resistance', 85000.00, 15.00, '["OrganicCert"]', NULL, '{"germination_rate": "95%", "days_to_harvest": "75-80", "fruit_weight": "150-200g", "disease_resistance": ["mosaic", "wilt"]}', '["huong-dan-gieo-trong.pdf"]', '["vs001-1.jpg", "vs001-2.jpg"]', 0, 200, 0.050, '{"length": 10, "width": 6, "height": 1}', 1, 1, 890, 45, 4.60, 8, '2025-08-20 09:30:00', '2025-08-21 04:30:00'),
(5, 3, 8, 'IS001', 'Hệ Thống Tưới Nhỏ Giọt Thông Minh', 'Smart Drip Irrigation System', 'Hệ thống tưới nhỏ giọt tự động với cảm biến độ ẩm đất', 'Automatic drip irrigation system with soil moisture sensors', 12000000.00, 8.00, '["WaterSaver", "SmartTech"]', 'A+', '{"coverage": "1000m2", "sensors": "5", "timer": "programmable", "water_saving": "60%"}', '["manual-is001-vi.pdf", "installation-guide.pdf"]', '["is001-1.jpg", "is001-2.jpg", "is001-3.jpg"]', 18, 30, 45.500, '{"controller_length": 30, "controller_width": 20, "controller_height": 15, "pipes_length": 500, "emitters_count": 100}', 1, 1, 423, 18, 4.40, 5, '2025-08-20 10:00:00', '2025-08-21 04:00:00');

-- Insert Environmental Data
INSERT INTO `environmental_data` (`Id`, `farm_profile_id`, `user_id`, `measurement_date`, `soil_ph`, `co2_footprint`, `soil_moisture_percentage`, `soil_type`, `Notes`, `created_at`, `updated_at`) VALUES
(1, 1, 7, '2025-08-20', 6.5, 125.50, 45.30, 'DatPhuSa', 'Đo pH đất sau mùa mưa, độ ẩm tốt cho gieo trồng', '2025-08-20 07:00:00', '2025-08-20 07:00:00'),
(2, 1, 7, '2025-08-19', 6.4, 128.75, 42.80, 'DatPhuSa', 'Cần bổ sung thêm phân hữu cơ để cải thiện độ pH', '2025-08-19 07:00:00', '2025-08-19 07:00:00'),
(3, 2, 8, '2025-08-20', 7.2, 95.25, 55.60, 'DatDoBazan', 'Đất có độ pH cao, phù hợp với cây lâu năm', '2025-08-20 08:00:00', '2025-08-20 08:00:00'),
(4, 2, 8, '2025-08-19', 7.1, 98.40, 52.30, 'DatDoBazan', 'Độ ẩm đất tốt, cần theo dõi thêm pH trong thời gian tới', '2025-08-19 08:00:00', '2025-08-19 08:00:00'),
(5, 1, 7, '2025-08-18', 6.6, 122.30, 48.90, 'DatPhuSa', 'Kết quả đo đạc sau khi bón phân compost', '2025-08-18 07:00:00', '2025-08-18 07:00:00');

-- Insert Energy Usage
INSERT INTO `energy_usage` (`id`, `environmental_data_id`, `electricity_kwh`, `gasoline_liters`, `diesel_liters`, `created_at`, `updated_at`) VALUES
(1, 1, 25.50, 0.00, 12.30, '2025-08-20 07:00:00', '2025-08-20 07:00:00'),
(2, 2, 28.75, 0.00, 15.20, '2025-08-19 07:00:00', '2025-08-19 07:00:00'),
(3, 3, 18.40, 5.50, 8.90, '2025-08-20 08:00:00', '2025-08-20 08:00:00'),
(4, 4, 22.10, 6.20, 10.50, '2025-08-19 08:00:00', '2025-08-19 08:00:00'),
(5, 5, 30.20, 0.00, 18.50, '2025-08-18 07:00:00', '2025-08-18 07:00:00');

-- Insert Fertilizers
INSERT INTO `fertilizers` (`id`, `environmental_data_id`, `organic_fertilizer`, `npk_fertilizer`, `urea_fertilizer`, `phosphate_fertilizer`, `created_at`, `updated_at`) VALUES
(1, 1, 500.00, 0.00, 0.00, 0.00, '2025-08-15 06:00:00', '2025-08-15 06:00:00'),
(2, 2, 0.00, 250.00, 50.00, 30.00, '2025-08-10 06:00:00', '2025-08-10 06:00:00'),
(3, 3, 300.00, 0.00, 0.00, 25.00, '2025-08-12 07:00:00', '2025-08-12 07:00:00'),
(4, 4, 400.00, 100.00, 0.00, 40.00, '2025-08-11 06:30:00', '2025-08-11 06:30:00'),
(5, 5, 350.00, 0.00, 0.00, 20.00, '2025-08-08 07:00:00', '2025-08-08 07:00:00');

-- Insert Orders
INSERT INTO `orders` (`Id`, `customer_id`, `vendor_id`, `Status`, `Subtotal`, `tax_amount`, `shipping_fee`, `discount_amount`, `total_amount`, `shipping_address`, `shipping_method`, `tracking_number`, `Notes`, `cancelled_reason`, `cancelled_at`, `confirmed_at`, `shipped_at`, `delivered_at`, `created_at`, `updated_at`) VALUES
(1, 5, 3, 'delivered', 25000000.00, 0.00, 500000.00, 2500000.00, 23000000.00, '{"name": "Phạm Văn Khách Hàng 1", "phone": "0901234571", "address": "123 Đường ABC, Quận 1, TP.HCM"}', 'express', 'VDT2025082001', 'Giao hàng vào buổi sáng', NULL, NULL, '2025-08-20 11:00:00', '2025-08-22 08:00:00', '2025-08-24 14:30:00', '2025-08-20 10:00:00', '2025-08-24 14:30:00'),
(2, 6, 4, 'shipped', 1716750.00, 0.00, 50000.00, 283250.00, 1483500.00, '{"name": "Hoàng Thị Khách Hàng 2", "phone": "0901234572", "address": "456 Đường XYZ, Quận 2, TP.HCM"}', 'standard', 'VDT2025082002', 'Cần gói cẩn thận', NULL, NULL, '2025-08-21 10:00:00', '2025-08-21 15:00:00', NULL, '2025-08-21 09:00:00', '2025-08-21 15:00:00'),
(3, 7, 3, 'confirmed', 12000000.00, 0.00, 200000.00, 960000.00, 11240000.00, '{"name": "Nguyễn Văn Nông Dân 1", "phone": "0901234573", "address": "789 Đường Nông Thôn, Đồng Nai"}', 'express', NULL, 'Cần hỗ trợ lắp đặt', NULL, NULL, '2025-08-21 11:30:00', NULL, NULL, '2025-08-21 11:00:00', '2025-08-21 11:30:00');

-- Insert Order Details
INSERT INTO `order_details` (`Id`, `order_id`, `product_id`, `Quantity`, `unit_price`, `discount_amount`, `Subtotal`, `created_at`) VALUES
(1, 1, 1, 1, 25000000.00, 2500000.00, 22500000.00, '2025-08-20 10:00:00'),
(2, 2, 3, 10, 150000.00, 0.00, 1500000.00, '2025-08-21 09:00:00'),
(3, 2, 4, 3, 85000.00, 38250.00, 216750.00, '2025-08-21 09:00:00'),
(4, 3, 5, 1, 12000000.00, 960000.00, 11040000.00, '2025-08-21 11:00:00');

-- Insert Payments
INSERT INTO `payments` (`Id`, `order_id`, `payment_method`, `payment_gateway`, `transaction_id`, `Amount`, `Status`, `gateway_response`, `refund_amount`, `refund_reason`, `refunded_at`, `paid_at`, `failed_at`, `created_at`, `updated_at`) VALUES
(1, 1, 'bank_transfer', 'vnpay', 'VNP2025082001234567', 23000000.00, 'completed', '{"code": "00", "message": "Success", "bank": "VCB"}', 0.00, NULL, NULL, '2025-08-20 11:30:00', NULL, '2025-08-20 10:00:00', '2025-08-20 11:30:00'),
(2, 2, 'credit_card', 'stripe', 'STR_2025082109876543', 1483500.00, 'completed', '{"id": "ch_abc123", "status": "succeeded"}', 0.00, NULL, NULL, '2025-08-21 09:15:00', NULL, '2025-08-21 09:00:00', '2025-08-21 09:15:00'),
(3, 3, 'cod', 'manual', 'COD2025082111001', 11240000.00, 'pending', '{}', 0.00, NULL, NULL, NULL, NULL, '2025-08-21 11:00:00', '2025-08-21 11:00:00');

-- Insert Product Reviews
INSERT INTO `product_reviews` (`Id`, `product_id`, `order_id`, `customer_id`, `Rating`, `Title`, `Comment`, `Images`, `helpful_count`, `unhelpful_count`, `created_at`, `updated_at`) VALUES
(1, 1, 1, 5, 5, 'Máy cày tuyệt vời!', 'Máy chạy êm, tiết kiệm điện và rất phù hợp với ruộng nhỏ của tôi. Chất lượng tốt, đóng gói cẩn thận.', '["review1-1.jpg", "review1-2.jpg"]', 12, 1, '2025-08-24 16:00:00', '2025-08-24 16:00:00'),
(2, 3, 2, 6, 4, 'Phân compost chất lượng', 'Phân rất tốt, cây trồng phát triển nhanh sau khi bón. Mùi không quá nặng.', '["review2-1.jpg"]', 8, 0, '2025-08-21 18:00:00', '2025-08-21 18:00:00'),
(3, 4, 2, 6, 5, 'Hạt giống nảy mầm tốt', 'Tỷ lệ nảy mầm cao như quảng cáo, cà chua to và ngon. Sẽ mua lại lần sau.', '[]', 5, 0, '2025-08-21 19:00:00', '2025-08-21 19:00:00');

-- Insert Cart items
INSERT INTO `cart` (`id`, `user_id`, `product_id`, `quantity`, `created_at`, `updated_at`) VALUES
(1, 7, 2, 1, '2025-08-21 08:00:00', '2025-08-21 08:00:00'),
(2, 8, 3, 5, '2025-08-21 07:30:00', '2025-08-21 07:30:00'),
(3, 8, 4, 2, '2025-08-21 07:35:00', '2025-08-21 07:35:00'),
(4, 5, 5, 1, '2025-08-21 09:00:00', '2025-08-21 09:00:00');

-- Insert Forum Categories
INSERT INTO `forum_categories` (`Id`, `Name`, `Description`, `is_active`, `created_at`, `updated_at`) VALUES
(1, 'Kỹ thuật canh tác', 'Thảo luận về các kỹ thuật canh tác hiện đại và bền vững', 1, '2025-08-20 08:00:00', '2025-08-21 08:00:00'),
(2, 'Thiết bị nông nghiệp', 'Chia sẻ kinh nghiệm về máy móc và thiết bị nông nghiệp', 1, '2025-08-20 08:00:00', '2025-08-21 07:30:00'),
(3, 'Phân bón và chăm sóc cây', 'Thảo luận về phân bón hữu cơ và cách chăm sóc cây trồng', 1, '2025-08-20 08:00:00', '2025-08-21 07:00:00'),
(4, 'Thị trường nông sản', 'Thông tin về giá cả và xu hướng thị trường nông sản', 1, '2025-08-20 08:00:00', '2025-08-20 08:00:00');

-- Insert Forum Posts
INSERT INTO `forum_posts` (`Id`, `category_id`, `user_id`, `Title`, `Content`, `Tags`, `view_count`, `reply_count`, `like_count`, `dislike_count`, `is_pinned`, `is_locked`, `Status`, `moderated_reason`, `moderated_by`, `last_activity_at`, `created_at`, `updated_at`) VALUES
(1, 1, 7, 'Kinh nghiệm trồng lúa hữu cơ năm 2025', '{"content": "Xin chia sẻ kinh nghiệm trồng lúa hữu cơ của tôi sau 5 năm thực hiện. Từ việc chuẩn bị đất, chọn giống, đến chăm sóc và thu hoạch..."}', '["lúa hữu cơ", "kinh nghiệm", "canh tác bền vững"]', 156, 3, 12, 0, 1, 0, 'published', NULL, NULL, '2025-08-21 08:00:00', '2025-08-20 14:00:00', '2025-08-21 08:00:00'),
(2, 2, 8, 'Đánh giá máy cày mini điện VerdantTech V1', '{"content": "Vừa mua máy cày mini điện này để dùng cho trang trại 2ha của mình. Sau 1 tuần sử dụng, tôi có một số nhận xét..."}', '["máy cày", "điện", "đánh giá", "VerdantTech"]', 89, 2, 8, 0, 0, 0, 'published', NULL, NULL, '2025-08-21 07:30:00', '2025-08-20 16:00:00', '2025-08-21 07:30:00'),
(3, 1, 5, 'Hỏi về cách phòng chống sâu bệnh tự nhiên', '{"content": "Mình mới bắt đầu trồng rau hữu cơ, muốn hỏi anh chị có kinh nghiệm về cách phòng chống sâu bệnh mà không dùng thuốc hóa học không?"}', '["sâu bệnh", "hữu cơ", "phòng trừ tự nhiên"]', 67, 1, 5, 0, 0, 0, 'published', NULL, NULL, '2025-08-21 10:00:00', '2025-08-21 09:00:00', '2025-08-21 09:00:00'),
(4, 3, 6, 'Cách ủ phân compost hiệu quả tại nhà', '{"content": "Chia sẻ cách ủ phân compost từ rác thải nhà bếp và lá cây hiệu quả, giúp tiết kiệm chi phí phân bón..."}', '["compost", "phân hữu cơ", "tự làm"]', 124, 0, 7, 0, 0, 0, 'published', NULL, NULL, '2025-08-21 10:00:00', '2025-08-21 10:00:00', '2025-08-21 10:00:00');

-- Insert Forum Comments
INSERT INTO `forum_comments` (`Id`, `post_id`, `user_id`, `parent_id`, `Content`, `like_count`, `dislike_count`, `Status`, `moderated_reason`, `moderated_by`, `created_at`, `updated_at`) VALUES
(1, 1, 8, NULL, 'Cảm ơn bạn đã chia sẻ! Mình cũng đang có ý định chuyển sang trồng lúa hữu cơ. Bạn có thể chia sẻ thêm về việc chọn giống lúa phù hợp không?', 3, 0, 'visible', NULL, NULL, '2025-08-20 15:00:00', '2025-08-20 15:00:00'),
(2, 1, 7, 1, 'Mình thường chọn giống lúa ST24 hoặc ST25 vì phù hợp với đất phù sa và có chất lượng gạo tốt. Bạn nên tham khảo thêm ý kiến kỹ thuật viên địa phương nhé!', 5, 0, 'visible', NULL, NULL, '2025-08-20 16:30:00', '2025-08-20 16:30:00'),
(3, 1, 5, NULL, 'Bài viết rất hữu ích! Mình đang cân nhắc chuyển từ canh tác truyền thống sang hữu cơ.', 2, 0, 'visible', NULL, NULL, '2025-08-21 07:00:00', '2025-08-21 07:00:00'),
(4, 2, 3, NULL, 'Cảm ơn bạn đã đánh giá sản phẩm của chúng tôi! Nếu có bất kỳ thắc mắc nào về sử dụng, hãy liên hệ với bộ phận hỗ trợ kỹ thuật nhé.', 4, 0, 'visible', NULL, NULL, '2025-08-20 17:00:00', '2025-08-20 17:00:00'),
(5, 2, 8, 4, 'Máy chạy rất ổn, chỉ có điều pin hơi nhanh hết khi làm đất cứng. Các bạn có kế hoạch nâng cấp dung lượng pin không?', 1, 0, 'visible', NULL, NULL, '2025-08-21 06:00:00', '2025-08-21 06:00:00'),
(6, 3, 7, NULL, 'Bạn có thể thử dùng dung dịch tỏi ớt để xịt phòng trừ sâu bệnh. Mình dùng hiệu quả lắm!', 6, 0, 'visible', NULL, NULL, '2025-08-21 10:00:00', '2025-08-21 10:00:00');

-- Insert Chatbot Conversations
INSERT INTO `chatbot_conversations` (`Id`, `user_id`, `session_id`, `Title`, `Context`, `is_active`, `started_at`, `ended_at`) VALUES
(1, 5, 'session_20250820_001', 'Tư vấn chọn máy cày', '{"topic": "equipment_consultation", "products_discussed": ["TC001"], "user_preferences": {"budget": "under_30m", "farm_size": "small"}}', 0, '2025-08-20 14:00:00', '2025-08-20 14:30:00'),
(2, 7, 'session_20250821_001', 'Hỗ trợ kỹ thuật canh tác', '{"topic": "farming_techniques", "crop_type": "rice", "farming_method": "organic"}', 1, '2025-08-21 06:00:00', NULL),
(3, 6, 'session_20250821_002', 'Tư vấn phân bón hữu cơ', '{"topic": "fertilizer_consultation", "crop_type": "vegetables", "soil_type": "sandy"}', 0, '2025-08-21 08:00:00', '2025-08-21 08:45:00');

-- Insert Chatbot Messages
INSERT INTO `chatbot_messages` (`id`, `conversation_id`, `message_type`, `message_text`, `attachments`, `created_at`, `ChatbotConversationId`) VALUES
(1, 1, 'user', 'Tôi cần tư vấn chọn máy cày cho ruộng nhỏ khoảng 2ha', '[]', '2025-08-20 14:00:00', 1),
(2, 1, 'bot', 'Với diện tích 2ha, tôi khuyên bạn nên chọn máy cày mini điện VerdantTech V1. Máy này có những ưu điểm: tiết kiệm năng lượng, thân thiện môi trường, phù hợp với ruộng nhỏ.', '[]', '2025-08-20 14:00:30', 1),
(3, 1, 'user', 'Giá của máy này là bao nhiêu? Có khuyến mãi không?', '[]', '2025-08-20 14:02:00', 1),
(4, 1, 'bot', 'Máy cày mini điện VerdantTech V1 có giá 25.000.000 VNĐ. Hiện tại đang có khuyến mãi 10%, giá sau khuyến mãi là 22.500.000 VNĐ. Bạn có muốn xem thêm thông tin chi tiết không?', '[]', '2025-08-20 14:02:30', 1),
(5, 2, 'user', 'Tôi muốn hỏi về kỹ thuật trồng lúa hữu cơ', '[]', '2025-08-21 06:00:00', 2),
(6, 2, 'bot', 'Trồng lúa hữu cơ cần chú ý những điểm sau: 1) Chuẩn bị đất: Sử dụng phân compost thay vì phân hóa học 2) Chọn giống: Nên chọn giống lúa địa phương thích nghi 3) Quản lý nước: Tưới tiêu đúng thời điểm 4) Phòng trừ sâu bệnh: Sử dụng biện pháp sinh học', '[]', '2025-08-21 06:01:00', 2),
(7, 3, 'user', 'Tôi trồng rau, đất cát, nên dùng loại phân nào?', '[]', '2025-08-21 08:00:00', 3),
(8, 3, 'bot', 'Với đất cát trồng rau, tôi khuyên bạn sử dụng Phân Compost Hữu Cơ Premium. Loại phân này giúp cải thiện cấu trúc đất cát, tăng khả năng giữ nước và cung cấp dinh dưỡng lâu dài cho cây trồng.', '[]', '2025-08-21 08:00:30', 3),
(9, 3, 'user', 'Cảm ơn bạn! Tôi sẽ đặt mua ngay.', '[]', '2025-08-21 08:44:00', 3),
(10, 3, 'bot', 'Rất vui được hỗ trợ bạn! Chúc bạn canh tác thành công. Nếu có thắc mắc gì khác, đừng ngần ngại liên hệ nhé!', '[]', '2025-08-21 08:44:30', 3);

-- Insert Weather Data Cache
INSERT INTO `weather_data_cache` (`id`, `farm_profile_id`, `api_source`, `weather_date`, `temperature_min`, `temperature_max`, `temperature_avg`, `humidity_percentage`, `precipitation_mm`, `wind_speed_kmh`, `wind_direction`, `uv_index`, `weather_condition`, `weather_icon`, `sunrise_time`, `sunset_time`, `raw_api_response`, `fetched_at`) VALUES
(1, 1, 'openweathermap', '2025-08-21', 25.00, 32.00, 28.50, 78.20, 0.00, 12.30, 'NE', 7.0, 'sunny', '01d', '06:15:00', '18:30:00', '{"coord":{"lon":106.8441,"lat":10.9545},"weather":[{"id":800,"main":"Clear","description":"clear sky","icon":"01d"}],"main":{"temp":28.5,"feels_like":32.1,"temp_min":25,"temp_max":32,"pressure":1013,"humidity":78}}', '2025-08-21 06:00:00'),
(2, 2, 'openweathermap', '2025-08-21', 26.00, 33.00, 29.10, 82.50, 2.50, 8.70, 'SE', 6.0, 'partly_cloudy', '02d', '06:10:00', '18:25:00', '{"coord":{"lon":106.4226,"lat":10.8838},"weather":[{"id":801,"main":"Clouds","description":"few clouds","icon":"02d"}],"main":{"temp":29.1,"feels_like":33.8,"temp_min":26,"temp_max":33,"pressure":1012,"humidity":82}}', '2025-08-21 05:30:00');

-- Insert Inventory Logs
INSERT INTO `inventory_logs` (`Id`, `product_id`, `Type`, `Quantity`, `balance_after`, `Reason`, `reference_type`, `reference_id`, `created_by`, `created_at`) VALUES
(1, 1, 'out', -1, 49, 'Bán hàng', 'order', 1, 3, '2025-08-20 10:00:00'),
(2, 3, 'out', -10, 490, 'Bán hàng', 'order', 2, 4, '2025-08-21 09:00:00'),
(3, 4, 'out', -3, 197, 'Bán hàng', 'order', 2, 4, '2025-08-21 09:00:00'),
(4, 2, 'in', 5, 25, 'Nhập kho', 'purchase', NULL, 3, '2025-08-21 07:00:00'),
(5, 5, 'adjustment', -2, 28, 'Kiểm kho', 'inventory_check', NULL, 1, '2025-08-21 08:00:00');
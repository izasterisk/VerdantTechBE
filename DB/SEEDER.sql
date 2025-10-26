-- SEEDER DATA FOR VERDANTTECH DATABASE v8.0
-- All passwords are: $2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS
-- Updated for schema v8.0: Removed address_id from users, added user_addresses junction table for many-to-many relationship
-- Updated environmental_data table: renamed soil_ph to phh2o, removed soil_type enum, added new environmental measurement fields
-- Dates adjusted to be recent as of 2025-09-22, ensured foreign key consistency

-- Insert Addresses (updated with FPT University HCM address for admin and real Vietnamese address data)
INSERT INTO `addresses` (`id`, `location_address`, `province`, `district`, `commune`, `province_code`, `district_code`, `commune_code`, `latitude`, `longitude`, `created_at`, `updated_at`) VALUES
(1, 'Lô E2a-7, Đường D1, Khu Công nghệ cao, Phường Long Thạnh Mỹ', 'TP.HCM', 'Thành phố Thủ Đức', 'Phường Long Thạnh Mỹ', '700000', '720300', '12653', 10.84142000, 106.80986000, '2025-09-22 07:45:00', '2025-09-22 07:45:00'),
(2, 'Số 789 Đường Công Nghiệp, Quận Hai Bà Trưng, Hà Nội', 'Hà Nội', 'Quận Hai Bà Trưng', 'Phường Bách Khoa', '100000', '100300', '20', 21.00650000, 105.84250000, '2025-09-22 08:00:00', '2025-09-22 08:00:00'),
(3, '65 Lê Văn Lương, Phường Tân Phong, Quận 7', 'Hồ Chí Minh', 'Quận 7', 'Phường Tân Phong', '700000', '700700', '9233', 10.73020000, 106.72250000, '2025-09-22 08:30:00', '2025-09-22 08:30:00'),
(4, '2 Hải Triều, Phường Bến Nghé, Quận 1', 'Hồ Chí Minh', 'Quận 1', 'Phường Bến Nghé', '700000', '700100', '8955', 10.77100000, 106.70500000, '2025-09-22 10:00:00', '2025-09-22 10:00:00'),
(5, '458 Minh Khai (Times City), Phường Vĩnh Tuy, Quận Hai Bà Trưng', 'Hà Nội', 'Quận Hai Bà Trưng', 'Phường Vĩnh Tuy', '100000', '100300', '20', 20.99700000, 105.87000000, '2025-09-22 10:30:00', '2025-09-22 10:30:00'),
(6, '18 Phan Đình Phùng, Phường Quán Thánh, Quận Ba Đình', 'Hà Nội', 'Quận Ba Đình', 'Phường Quán Thánh', '100000', '100100', '63', 21.04090000, 105.84100000, '2025-09-22 11:00:00', '2025-09-22 11:00:00'),
(7, '720A Điện Biên Phủ, Phường 22, Quận Bình Thạnh', 'Hồ Chí Minh', 'Quận Bình Thạnh', 'Phường 22', '700000', '701600', '8989', 10.80200000, 106.72000000, '2025-09-22 11:30:00', '2025-09-22 11:30:00'),
(8, '72A Nguyễn Trãi (Royal City), Phường Thượng Đình, Quận Thanh Xuân', 'Hà Nội', 'Quận Thanh Xuân', 'Phường Thượng Đình', '100000', '100700', '78', 21.00370000, 105.81560000, '2025-09-22 12:00:00', '2025-09-22 12:00:00'),
(9, '2 Trường Sơn, Phường 2, Quận Tân Bình', 'Hồ Chí Minh', 'Quận Tân Bình', 'Phường 2', '700000', '701400', '9110', 10.81100000, 106.66000000, '2025-09-22 12:30:00', '2025-09-22 12:30:00');

-- Insert Users (with gmail.com emails, consistent password, and vendor role users)
INSERT INTO `users` (`id`, `email`, `password_hash`, `role`, `full_name`, `phone_number`, `tax_code`, `is_verified`, `verification_token`, `verification_sent_at`, `avatar_url`, `status`, `last_login_at`, `RefreshToken`, `RefreshTokenExpiresAt`, `created_at`, `updated_at`, `deleted_at`) VALUES
(1, 'admin@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'admin', 'Quản trị viên hệ thống', '0901234567', NULL, 1, NULL, NULL, NULL, 'active', '2025-09-22 08:00:00', NULL, NULL, '2025-09-22 07:00:00', '2025-09-22 08:00:00', NULL),
(2, 'staff1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'staff', 'Nguyễn Văn Nhân Viên 1', '0901234568', NULL, 1, NULL, NULL, NULL, 'active', '2025-09-22 07:30:00', NULL, NULL, '2025-09-22 07:00:00', '2025-09-22 07:30:00', NULL),
(3, 'staff2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'staff', 'Trần Thị Nhân Viên 2', '0901234569', NULL, 1, NULL, NULL, NULL, 'active', '2025-09-22 07:00:00', NULL, NULL, '2025-09-22 08:00:00', '2025-09-22 07:00:00', NULL),
(4, 'staff3@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'staff', 'Lê Văn Nhân Viên 3', '0901234570', NULL, 1, NULL, NULL, NULL, 'active', '2025-09-22 06:30:00', NULL, NULL, '2025-09-22 08:30:00', '2025-09-22 06:30:00', NULL),
-- Vendor users (NEW in v7)
(5, 'vendor1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Thiết Bị Nông Nghiệp Xanh', '0901234571', 'MST123456789', 1, NULL, NULL, NULL, 'active', '2025-09-22 08:15:00', NULL, NULL, '2025-09-22 09:00:00', '2025-09-22 08:15:00', NULL),
(6, 'vendor2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Cửa Hàng Nông Sản Sạch VerdantTech', '0901234572', 'MST987654321', 1, NULL, NULL, NULL, 'active', '2025-09-22 08:10:00', NULL, NULL, '2025-09-22 09:30:00', '2025-09-22 08:10:00', NULL),
-- Customer users
(7, 'customer1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Phạm Văn Khách Hàng 1', '0901234573', NULL, 1, NULL, NULL, NULL, 'active', '2025-09-22 08:15:00', NULL, NULL, '2025-09-22 09:00:00', '2025-09-22 08:15:00', NULL),
(8, 'customer2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Hoàng Thị Khách Hàng 2', '0901234574', NULL, 1, NULL, NULL, NULL, 'active', '2025-09-22 08:10:00', NULL, NULL, '2025-09-22 09:30:00', '2025-09-22 08:10:00', NULL),
(9, 'farmer1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Nguyễn Văn Nông Dân 1', '0901234575', NULL, 1, NULL, NULL, NULL, 'active', '2025-09-22 06:00:00', NULL, NULL, '2025-09-22 10:00:00', '2025-09-22 06:00:00', NULL),
(10, 'farmer2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Trần Thị Nông Dân 2', '0901234576', NULL, 1, NULL, NULL, NULL, 'active', '2025-09-22 05:30:00', NULL, NULL, '2025-09-22 10:30:00', '2025-09-22 05:30:00', NULL),
(11, 'testuser@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Người Dùng Test', '0901234577', NULL, 0, 'test-token-123', '2025-09-22 07:00:00', NULL, 'active', NULL, NULL, NULL, '2025-09-22 07:00:00', '2025-09-22 07:00:00', NULL),
(12, 'inactive@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Người Dùng Không Hoạt Động', '0901234578', NULL, 1, NULL, NULL, NULL, 'inactive', '2025-09-22 15:00:00', NULL, NULL, '2025-09-22 11:00:00', '2025-09-22 15:00:00', NULL);

-- Insert User Addresses (NEW in v7.2 - junction table for many-to-many relationship between users and addresses)
INSERT INTO `user_addresses` (`id`, `user_id`, `address_id`, `is_deleted`, `created_at`, `updated_at`, `deleted_at`) VALUES
-- Admin address (FPT University HCM)
(1, 1, 1, 0, '2025-09-22 07:45:00', '2025-09-22 07:45:00', NULL), -- Admin -> Address 1 (FPT University HCM)
-- Vendor addresses
(2, 5, 2, 0, '2025-09-22 09:00:00', '2025-09-22 09:00:00', NULL), -- Vendor 1 -> Address 2 (Hà Nội)
(3, 6, 3, 0, '2025-09-22 09:30:00', '2025-09-22 09:30:00', NULL), -- Vendor 2 -> Address 3 (TP.HCM)
-- Farmer addresses
(4, 9, 4, 0, '2025-09-22 10:00:00', '2025-09-22 10:00:00', NULL), -- Farmer 1 -> Address 4 (Đồng Nai)
(5, 10, 5, 0, '2025-09-22 10:30:00', '2025-09-22 10:30:00', NULL), -- Farmer 2 -> Address 5 (Long An)
-- Additional addresses for users (demonstrating many-to-many capability)
(6, 7, 6, 0, '2025-09-22 11:00:00', '2025-09-22 11:00:00', NULL), -- Customer 1 -> Address 6 (Mê Linh, Hà Nội)
(7, 8, 7, 0, '2025-09-22 11:30:00', '2025-09-22 11:30:00', NULL), -- Customer 2 -> Address 7 (Tân Bình, TP.HCM)
(8, 8, 8, 0, '2025-09-22 12:00:00', '2025-09-22 12:00:00', NULL), -- Customer 2 also has Address 8 (Hà Đông, Hà Nội)
(9, 9, 9, 0, '2025-09-22 12:30:00', '2025-09-22 12:30:00', NULL), -- Farmer 1 also has Address 9 (Thạch Đà, Mê Linh)
(10, 2, 3, 0, '2025-09-22 13:30:00', '2025-09-22 13:30:00', NULL); -- Staff 1 -> Address 3 (TP.HCM)

-- Insert Vendor Profiles (for vendor users)
INSERT INTO `vendor_profiles` (`id`, `user_id`, `company_name`, `slug`, `business_registration_number`, `verified_at`, `verified_by`, `created_at`, `updated_at`) VALUES
(1, 5, 'Công Ty Thiết Bị Nông Nghiệp Xanh', 'cong-ty-thiet-bi-nong-nghiep-xanh', 'BRN123456789', '2025-09-09 07:00:00', 1, '2025-09-08 08:00:00', '2025-09-09 07:00:00'),
(2, 6, 'Cửa Hàng Nông Sản Sạch VerdantTech', 'cua-hang-nong-san-sach-verdanttech', 'BRN987654321', '2025-09-09 06:30:00', 1, '2025-09-08 08:30:00', '2025-09-09 06:30:00');

-- Insert Vendor Bank Accounts (v7.1 structure with direct bank_code and vendor_id referencing users table)
INSERT INTO `vendor_bank_accounts` (`id`, `vendor_id`, `bank_code`, `account_number`, `account_holder`, `is_default`, `created_at`, `updated_at`) VALUES
(1, 5, 'VCB', '1234567890', 'Công Ty Thiết Bị Nông Nghiệp Xanh', 1, '2025-09-09 07:05:00', '2025-09-09 07:05:00'),
(2, 6, 'ACB', '0987654321', 'Cửa Hàng Nông Sản Sạch VerdantTech', 1, '2025-09-09 06:35:00', '2025-09-09 06:35:00');

-- Insert Wallets (v7.1 structure - removed pending_withdraw, added last_transaction_id and last_updated_by)
INSERT INTO `wallets` (`id`, `vendor_id`, `balance`, `last_transaction_id`, `last_updated_by`, `created_at`, `updated_at`) VALUES
(1, 5, 10000000.00, NULL, 1, '2025-09-09 08:00:00', '2025-09-09 08:00:00'),
(2, 6, 2500000.00, NULL, 1, '2025-09-09 08:00:00', '2025-09-09 08:00:00');

-- Insert Vendor Certificates (v8.1 structure - removed media fields, use media_links table instead)
INSERT INTO `vendor_certificates` (`id`, `vendor_id`, `certification_code`, `certification_name`, `status`, `rejection_reason`, `uploaded_at`, `verified_at`, `verified_by`, `created_at`, `updated_at`) VALUES
-- Vendor 1 (Công Ty Thiết Bị Nông Nghiệp Xanh) certificates
(1, 5, 'ISO14001', 'ISO 14001 Environmental Management', 'verified', NULL, '2025-09-08 09:00:00', '2025-09-09 07:00:00', 1, '2025-09-08 09:00:00', '2025-09-09 07:00:00'),
(2, 5, 'ISO50001', 'ISO 50001 Energy Management', 'verified', NULL, '2025-09-08 09:15:00', '2025-09-09 07:00:00', 1, '2025-09-08 09:15:00', '2025-09-09 07:00:00'),
(3, 5, 'CARBON_NEUTRAL', 'Carbon Neutral Certification', 'verified', NULL, '2025-09-08 09:30:00', '2025-09-09 07:00:00', 1, '2025-09-08 09:30:00', '2025-09-09 07:00:00'),
(4, 5, 'HACCP', 'HACCP - Hazard Analysis Critical Control Points', 'pending', NULL, '2025-09-09 08:00:00', NULL, NULL, '2025-09-09 08:00:00', '2025-09-09 08:00:00'),

-- Vendor 2 (Cửa Hàng Nông Sản Sạch VerdantTech) certificates  
(5, 6, 'USDA_ORGANIC', 'USDA Organic Certification', 'verified', NULL, '2025-09-08 10:00:00', '2025-09-09 06:30:00', 1, '2025-09-08 10:00:00', '2025-09-09 06:30:00'),
(6, 6, 'VIETGAP', 'VietGAP – Thực hành nông nghiệp tốt tại Việt Nam', 'verified', NULL, '2025-09-08 10:15:00', '2025-09-09 06:30:00', 1, '2025-09-08 10:15:00', '2025-09-09 06:30:00'),
(7, 6, 'FAIRTRADE', 'Fairtrade International Certification', 'verified', NULL, '2025-09-08 10:30:00', '2025-09-09 06:30:00', 1, '2025-09-08 10:30:00', '2025-09-09 06:30:00'),
(8, 6, 'NON_GMO', 'Non-GMO Project Verified', 'rejected', 'Chứng chỉ không rõ ràng, cần upload lại bản gốc', '2025-09-09 09:00:00', '2025-09-09 10:00:00', 1, '2025-09-09 09:00:00', '2025-09-09 10:00:00');

-- Insert Farm Profiles
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `primary_crops`, `status`, `created_at`, `updated_at`) VALUES
(1, 9, 'Trang trại Xanh Sạch Đồng Nai', 5.50, 4, 'Lúa, Rau xanh, Cà chua', 'Active', '2025-09-08 10:00:00', '2025-09-09 06:00:00'),
(2, 10, 'Trang trại Hữu Cơ Long An', 8.25, 5, 'Rau củ, Trái cây, Thảo dược', 'Active', '2025-09-08 10:30:00', '2025-09-09 05:30:00'),
(3, 9, 'Trang trại Thực nghiệm Mê Linh', 3.75, 9, 'Rau sạch, Hoa màu', 'Active', '2025-09-08 11:00:00', '2025-09-09 06:30:00');

-- Insert Product Categories
INSERT INTO `product_categories` (`id`, `parent_id`, `name`, `slug`, `description`, `is_active`, `created_at`, `updated_at`) VALUES
(1, NULL, 'Thiết Bị Nông Nghiệp', 'thiet-bi-nong-nghiep', 'Các loại máy móc và thiết bị phục vụ sản xuất nông nghiệp', 1, '2025-09-08 07:00:00', '2025-09-08 07:00:00'),
(2, 1, 'Máy Cày', 'may-cay', 'Máy cày và thiết bị làm đất', 1, '2025-09-08 07:00:00', '2025-09-08 07:00:00'),
(3, 1, 'Máy Gặt', 'may-gat', 'Máy gặt và thu hoạch', 1, '2025-09-08 07:00:00', '2025-09-08 07:00:00'),
(4, NULL, 'Hạt Giống', 'hat-giong', 'Hạt giống chất lượng cao', 1, '2025-09-08 07:00:00', '2025-09-08 07:00:00'),
(5, 4, 'Hạt Giống Rau', 'hat-giong-rau', 'Hạt giống rau củ hữu cơ', 1, '2025-09-08 07:00:00', '2025-09-08 07:00:00'),
(6, NULL, 'Phân Bón', 'phan-bon', 'Phân bón hữu cơ và hóa học', 1, '2025-09-08 07:00:00', '2025-09-08 07:00:00');

-- Insert Products (v8.1 structure - removed images, added public_url, use media_links table for images)
INSERT INTO `products` (`id`, `category_id`, `vendor_id`, `product_code`, `product_name`, `slug`, `description`, `unit_price`, `commission_rate`, `discount_percentage`, `energy_efficiency_rating`, `specifications`, `manual_urls`, `public_url`, `warranty_months`, `stock_quantity`, `weight_kg`, `dimensions_cm`, `is_active`, `for_rent`, `view_count`, `sold_count`, `rating_average`, `created_at`, `updated_at`) VALUES
(1, 2, 5, 'TC001', 'Máy Cày Mini Điện VerdantTech V1', 'may-cay-mini-dien-verdanttech-v1', 'Máy cày mini sử dụng năng lượng điện, thân thiện với môi trường, phù hợp cho nông trại nhỏ.', 25000000.00, 10.00, 5.00, 5, '{"power": "10kW", "battery": "48V 100Ah"}', 'manual_tc001.pdf', NULL, 24, 50, 500.000, '{"length": 250, "width": 120, "height": 150}', 1, 1, 120, 5, 4.60, '2025-09-08 07:00:00', '2025-09-09 07:00:00'),
(2, 3, 5, 'HV002', 'Máy Gặt Lúa Tự Động VerdantTech H2', 'may-gat-lua-tu-dong-verdanttech-h2', 'Máy gặt lúa tự động với công nghệ AI, tiết kiệm thời gian và công sức.', 150000000.00, 8.00, 5.00, 4, '{"engine": "Diesel 50HP", "capacity": "2 tons/hour"}', 'manual_hv002.pdf', NULL, 36, 10, 2500.000, '{"length": 450, "width": 200, "height": 250}', 1, 1, 85, 3, 4.80, '2025-09-08 07:15:00', '2025-09-09 07:15:00'),
(3, 5, 6, 'SD003', 'Hạt Giống Rau Cải Xanh Hữu Cơ', 'hat-giong-rau-cai-xanh-huu-co', 'Hạt giống rau cải xanh hữu cơ, tỷ lệ nảy mầm cao, kháng bệnh tốt.', 50000.00, 5.00, 5.00, NULL, '{"germination_rate": "95%", "pack_size": "100g"}', 'manual_sd003.pdf', NULL, 0, 200, 0.100, '{"length": 10, "width": 5, "height": 2}', 1, 0, 200, 50, 4.50, '2025-09-08 07:30:00', '2025-09-09 07:30:00'),
(4, 6, 6, 'FT004', 'Phân Bón Hữu Cơ Compost Premium', 'phan-bon-huu-co-compost-premium', 'Phân bón hữu cơ từ compost, giàu dinh dưỡng, an toàn cho đất và cây trồng.', 100000.00, 7.00, 5.00, NULL, '{"npk": "5-5-5", "weight": "25kg"}', 'manual_ft004.pdf', NULL, 0, 100, 25.000, '{"length": 50, "width": 30, "height": 10}', 1, 0, 150, 20, 4.70, '2025-09-08 07:45:00', '2025-09-09 07:45:00'),
(5, 1, 5, 'DR005', 'Drone Phun Thuốc Thông Minh VerdantTech D3', 'drone-phun-thuoc-thong-minh-verdanttech-d3', 'Drone phun thuốc tự động với AI, chính xác cao, tiết kiệm thuốc.', 30000000.00, 12.00, 5.00, 3, '{"flight_time": "30min", "capacity": "10L"}', 'manual_dr005.pdf', NULL, 12, 15, 5.000, '{"length": 100, "width": 100, "height": 50}', 1, 1, 90, 7, 4.40, '2025-09-08 08:00:00', '2025-09-09 08:00:00');

-- Insert Product Certificates (v8.1 structure - removed media fields, use media_links table instead)
INSERT INTO `product_certificates` (`id`, `product_id`, `certification_code`, `certification_name`, `status`, `rejection_reason`, `uploaded_at`, `verified_at`, `verified_by`, `created_at`, `updated_at`) VALUES
-- Product 1 (Máy Cày Mini Điện VerdantTech V1) certificates
(1, 1, 'ISO50001', 'ISO 50001 Energy Management', 'verified', NULL, '2025-09-08 09:00:00', '2025-09-09 07:00:00', 1, '2025-09-08 09:00:00', '2025-09-09 07:00:00'),
(2, 1, 'CARBON_NEUTRAL', 'Carbon Neutral Certification', 'verified', NULL, '2025-09-08 09:15:00', '2025-09-09 07:00:00', 1, '2025-09-08 09:15:00', '2025-09-09 07:00:00'),
-- Product 2 (Máy Gặt Lúa Tự Động VerdantTech H2) certificates
(3, 2, 'ISO14001', 'ISO 14001 Environmental Management', 'verified', NULL, '2025-09-08 09:30:00', '2025-09-09 07:00:00', 1, '2025-09-08 09:30:00', '2025-09-09 07:00:00'),
(4, 2, 'SBTI', 'SBTi - Science Based Targets Initiative', 'pending', NULL, '2025-09-09 08:00:00', NULL, NULL, '2025-09-09 08:00:00', '2025-09-09 08:00:00'),
-- Product 3 (Hạt Giống Rau Cải Xanh Hữu Cơ) certificates
(5, 3, 'USDA_ORGANIC', 'USDA Organic Certification', 'verified', NULL, '2025-09-08 10:00:00', '2025-09-09 06:30:00', 1, '2025-09-08 10:00:00', '2025-09-09 06:30:00'),
(6, 3, 'VIETGAP', 'VietGAP – Thực hành nông nghiệp tốt tại Việt Nam', 'verified', NULL, '2025-09-08 10:15:00', '2025-09-09 06:30:00', 1, '2025-09-08 10:15:00', '2025-09-09 06:30:00'),
-- Product 4 (Phân Bón Hữu Cơ Compost Premium) certificates
(7, 4, 'GLOBALGAP', 'GlobalGAP Certification', 'verified', NULL, '2025-09-08 10:30:00', '2025-09-09 06:30:00', 1, '2025-09-08 10:30:00', '2025-09-09 06:30:00'),
(8, 4, 'NON_GMO', 'Non-GMO Project Verified', 'rejected', 'Chứng chỉ không rõ ràng, cần upload lại bản gốc', '2025-09-09 09:00:00', '2025-09-09 10:00:00', 1, '2025-09-09 09:00:00', '2025-09-09 10:00:00'),
-- Product 5 (Drone Phun Thuốc Thông Minh VerdantTech D3) certificates
(9, 5, 'RAINFOREST_ALLIANCE', 'Rainforest Alliance Certification', 'verified', NULL, '2025-09-08 11:00:00', '2025-09-09 07:00:00', 1, '2025-09-08 11:00:00', '2025-09-09 07:00:00'),
(10, 5, 'CARBON_NEUTRAL_2', 'Carbon Neutral Certification', 'pending', NULL, '2025-09-09 08:30:00', NULL, NULL, '2025-09-09 08:30:00', '2025-09-09 08:30:00');

-- Insert Media Links (v8.1 structure - centralized media storage)
-- Media for products
INSERT INTO `media_links` (`id`, `owner_type`, `owner_id`, `image_url`, `image_public_id`, `purpose`, `sort_order`, `created_at`, `updated_at`) VALUES
-- Product 1 (Máy Cày Mini Điện VerdantTech V1) - 2 images
(1, 'products', 1, 'https://cloudinary.com/verdanttech/products/tc001_1.jpg', 'verdanttech/products/tc001_1', 'none', 1, '2025-09-08 07:00:00', '2025-09-08 07:00:00'),
(2, 'products', 1, 'https://cloudinary.com/verdanttech/products/tc001_2.jpg', 'verdanttech/products/tc001_2', 'none', 2, '2025-09-08 07:00:00', '2025-09-08 07:00:00'),
-- Product 2 (Máy Gặt Lúa Tự Động VerdantTech H2) - 2 images
(3, 'products', 2, 'https://cloudinary.com/verdanttech/products/hv002_1.jpg', 'verdanttech/products/hv002_1', 'none', 1, '2025-09-08 07:15:00', '2025-09-08 07:15:00'),
(4, 'products', 2, 'https://cloudinary.com/verdanttech/products/hv002_2.jpg', 'verdanttech/products/hv002_2', 'none', 2, '2025-09-08 07:15:00', '2025-09-08 07:15:00'),
-- Product 3 (Hạt Giống Rau Cải Xanh Hữu Cơ) - 1 image
(5, 'products', 3, 'https://cloudinary.com/verdanttech/products/sd003_1.jpg', 'verdanttech/products/sd003_1', 'none', 1, '2025-09-08 07:30:00', '2025-09-08 07:30:00'),
-- Product 4 (Phân Bón Hữu Cơ Compost Premium) - 2 images
(6, 'products', 4, 'https://cloudinary.com/verdanttech/products/ft004_1.jpg', 'verdanttech/products/ft004_1', 'none', 1, '2025-09-08 07:45:00', '2025-09-08 07:45:00'),
(7, 'products', 4, 'https://cloudinary.com/verdanttech/products/ft004_2.jpg', 'verdanttech/products/ft004_2', 'none', 2, '2025-09-08 07:45:00', '2025-09-08 07:45:00'),
-- Product 5 (Drone Phun Thuốc Thông Minh VerdantTech D3) - 2 images
(8, 'products', 5, 'https://cloudinary.com/verdanttech/products/dr005_1.jpg', 'verdanttech/products/dr005_1', 'none', 1, '2025-09-08 08:00:00', '2025-09-08 08:00:00'),
(9, 'products', 5, 'https://cloudinary.com/verdanttech/products/dr005_2.jpg', 'verdanttech/products/dr005_2', 'none', 2, '2025-09-08 08:00:00', '2025-09-08 08:00:00'),

-- Media for product_certificates
(10, 'product_certificates', 1, 'https://cloudinary.com/verdanttech/certificates/product1_iso50001.pdf', 'verdanttech/certificates/product1_iso50001', 'none', 1, '2025-09-08 09:00:00', '2025-09-09 07:00:00'),
(11, 'product_certificates', 2, 'https://cloudinary.com/verdanttech/certificates/product1_carbon_neutral.pdf', 'verdanttech/certificates/product1_carbon_neutral', 'none', 1, '2025-09-08 09:15:00', '2025-09-09 07:00:00'),
(12, 'product_certificates', 3, 'https://cloudinary.com/verdanttech/certificates/product2_iso14001.pdf', 'verdanttech/certificates/product2_iso14001', 'none', 1, '2025-09-08 09:30:00', '2025-09-09 07:00:00'),
(13, 'product_certificates', 4, 'https://cloudinary.com/verdanttech/certificates/product2_sbti.pdf', 'verdanttech/certificates/product2_sbti', 'none', 1, '2025-09-09 08:00:00', '2025-09-09 08:00:00'),
(14, 'product_certificates', 5, 'https://cloudinary.com/verdanttech/certificates/product3_usda_organic.pdf', 'verdanttech/certificates/product3_usda_organic', 'none', 1, '2025-09-08 10:00:00', '2025-09-09 06:30:00'),
(15, 'product_certificates', 6, 'https://cloudinary.com/verdanttech/certificates/product3_vietgap.pdf', 'verdanttech/certificates/product3_vietgap', 'none', 1, '2025-09-08 10:15:00', '2025-09-09 06:30:00'),
(16, 'product_certificates', 7, 'https://cloudinary.com/verdanttech/certificates/product4_globalgap.pdf', 'verdanttech/certificates/product4_globalgap', 'none', 1, '2025-09-08 10:30:00', '2025-09-09 06:30:00'),
(17, 'product_certificates', 8, 'https://cloudinary.com/verdanttech/certificates/product4_non_gmo.pdf', 'verdanttech/certificates/product4_non_gmo', 'none', 1, '2025-09-09 09:00:00', '2025-09-09 10:00:00'),
(18, 'product_certificates', 9, 'https://cloudinary.com/verdanttech/certificates/product5_rainforest_alliance.pdf', 'verdanttech/certificates/product5_rainforest_alliance', 'none', 1, '2025-09-08 11:00:00', '2025-09-09 07:00:00'),
(19, 'product_certificates', 10, 'https://cloudinary.com/verdanttech/certificates/product5_carbon_neutral.pdf', 'verdanttech/certificates/product5_carbon_neutral', 'none', 1, '2025-09-09 08:30:00', '2025-09-09 08:30:00'),

-- Media for product_reviews
(20, 'product_reviews', 1, 'https://cloudinary.com/verdanttech/reviews/review1-1.jpg', 'verdanttech/reviews/review1-1', 'none', 1, '2025-09-09 16:00:00', '2025-09-09 16:00:00'),
(21, 'product_reviews', 1, 'https://cloudinary.com/verdanttech/reviews/review1-2.jpg', 'verdanttech/reviews/review1-2', 'none', 2, '2025-09-09 16:00:00', '2025-09-09 16:00:00'),
(22, 'product_reviews', 2, 'https://cloudinary.com/verdanttech/reviews/review2-1.jpg', 'verdanttech/reviews/review2-1', 'none', 1, '2025-09-09 18:00:00', '2025-09-09 18:00:00');

-- Insert Cart (v7.1 structure - changed user_id to customer_id)
INSERT INTO `cart` (`id`, `customer_id`, `created_at`, `updated_at`) VALUES
(1, 7, '2025-09-15 08:00:00', '2025-09-15 08:30:00'),
(2, 8, '2025-09-15 09:00:00', '2025-09-15 09:15:00'),
(3, 9, '2025-09-15 10:00:00', '2025-09-15 10:00:00');

-- Insert Cart Items (new table in v7.1)
INSERT INTO `cart_items` (`id`, `cart_id`, `product_id`, `quantity`, `created_at`, `updated_at`) VALUES
(1, 1, 3, 5, '2025-09-15 08:00:00', '2025-09-15 08:00:00'),
(2, 1, 4, 2, '2025-09-15 08:15:00', '2025-09-15 08:15:00'),
(3, 2, 1, 1, '2025-09-15 09:00:00', '2025-09-15 09:00:00'),
(4, 2, 5, 1, '2025-09-15 09:10:00', '2025-09-15 09:10:00'),
(5, 3, 3, 10, '2025-09-15 10:00:00', '2025-09-15 10:00:00');

-- Insert Product Registrations (v7.1 structure - updated column names: product_code→proposed_product_code, name→proposed_product_name, price→unit_price, removed commission_rate, added approved_at)
-- INSERT INTO `product_registrations` (`id`, `vendor_id`, `category_id`, `proposed_product_code`, `proposed_product_name`, `description`, `unit_price`, `energy_efficiency_rating`, `specifications`, `manual_urls`, `images`, `warranty_months`, `weight_kg`, `dimensions_cm`, `status`, `rejection_reason`, `approved_by`, `approved_at`, `created_at`) VALUES
-- (1, 5, 2, 'TC002', 'Máy Cày Điện Eco V2', 'Phiên bản cải tiến của máy cày điện với pin lớn hơn', 28000000.00, 'A++', '{"power": "12kW", "battery": "48V 120Ah", "working_time": "6 hours"}', 'manual_eco_v2.pdf', 'eco_v2_1.jpg,eco_v2_2.jpg', 24, 520.000, '{"length": 260, "width": 125, "height": 155}', 'pending', NULL, NULL, NULL, '2025-09-15 07:00:00'),
-- (2, 6, 5, 'SD004', 'Hạt Giống Rau Spinach Premium', 'Hạt giống rau spinach hữu cơ cao cấp', 60000.00, NULL, '{"germination_rate": "98%", "pack_size": "50g", "origin": "Netherlands"}', 'manual_spinach.pdf', 'spinach_1.jpg', 0, 0.050, '{"length": 8, "width": 4, "height": 2}', 'approved', NULL, 1, '2025-09-15 08:00:00', '2025-09-14 14:00:00'),
-- (3, 5, 1, 'IR001', 'Hệ Thống Tưới Thông Minh AI', 'Hệ thống tưới tự động với AI để tối ưu hóa nước', 15000000.00, 'A', '{"coverage": "1 hectare", "sensors": "soil moisture, weather", "connectivity": "WiFi, 4G"}', 'manual_smart_irrigation.pdf', 'smart_irrigation_1.jpg,smart_irrigation_2.jpg', 36, 50.000, '{"length": 200, "width": 150, "height": 100}', 'rejected', 'Cần bổ sung thông tin kỹ thuật về cảm biến', 2, NULL, '2025-09-13 10:00:00');

-- Insert Forum Categories
INSERT INTO `forum_categories` (`id`, `name`, `description`, `is_active`, `created_at`, `updated_at`) VALUES
(1, 'Kỹ Thuật Canh Tác', 'Thảo luận về các phương pháp canh tác bền vững và hữu cơ', 1, '2025-09-08 07:00:00', '2025-09-08 07:00:00'),
(2, 'Thiết Bị Nông Nghiệp', 'Chia sẻ kinh nghiệm sử dụng máy móc và thiết bị nông nghiệp xanh', 1, '2025-09-08 07:00:00', '2025-09-08 07:00:00'),
(3, 'Phòng Trừ Sâu Bệnh', 'Các biện pháp phòng trừ sâu bệnh thân thiện với môi trường', 1, '2025-09-08 07:00:00', '2025-09-08 07:00:00');

-- Insert Forum Posts (v7.1 structure - changed category_id to forum_category_id, removed last_activity_at column)
INSERT INTO `forum_posts` (`id`, `forum_category_id`, `user_id`, `title`, `slug`, `content`, `tags`, `view_count`, `like_count`, `dislike_count`, `is_pinned`, `status`, `created_at`, `updated_at`) VALUES
(1, 1, 9, 'Kinh nghiệm trồng lúa hữu cơ tại Đồng Nai', 'kinh-nghiem-trong-lua-huu-co-tai-dong-nai', '[{"order": 1, "type": "text", "content": "Chào mọi người, mình đang trồng lúa hữu cơ tại Đồng Nai. Ai có kinh nghiệm chia sẻ nhé!"}, {"order": 2, "type": "image", "content": "https://example.com/images/lua-huu-co.jpg"}]', 'lúa, hữu cơ, đồng nai', 150, 20, 2, 1, 'visible', '2025-09-08 14:00:00', '2025-09-09 10:00:00'),
(2, 2, 7, 'Review máy cày mini điện VerdantTech V1', 'review-may-cay-mini-dien-verdanttech-v1', '[{"order": 1, "type": "text", "content": "Mình mới mua máy cày mini điện V1, chạy rất êm và tiết kiệm. Có ai dùng chưa?"}]', 'máy cày, điện, verdanttech', 80, 15, 1, 0, 'visible', '2025-09-08 15:00:00', '2025-09-09 06:00:00'),
(3, 3, 10, 'Biện pháp phòng sâu bệnh tự nhiên cho rau củ', 'bien-phap-phong-sau-benh-tu-nhien-cho-rau-cu', '[{"order": 1, "type": "text", "content": "Mọi người thường dùng gì để phòng sâu bệnh cho rau mà không dùng thuốc hóa học?"}]', 'sâu bệnh, rau củ, tự nhiên', 120, 18, 0, 0, 'visible', '2025-09-08 16:00:00', '2025-09-09 10:00:00');

-- Insert Forum Comments (v7.1 structure - changed post_id to forum_post_id)
INSERT INTO `forum_comments` (`id`, `forum_post_id`, `user_id`, `parent_id`, `content`, `like_count`, `dislike_count`, `status`, `created_at`, `updated_at`) VALUES
(1, 1, 10, NULL, 'Mình ở Long An cũng trồng lúa hữu cơ. Quan trọng là chọn giống lúa phù hợp không?', 3, 0, 'visible', '2025-09-08 15:00:00', '2025-09-08 15:00:00'),
(2, 1, 9, 1, 'Mình thường chọn giống lúa ST24 hoặc ST25 vì phù hợp với đất phù sa và có chất lượng gạo tốt. Bạn nên tham khảo thêm ý kiến kỹ thuật viên địa phương nhé!', 5, 0, 'visible', '2025-09-08 16:30:00', '2025-09-08 16:30:00'),
(3, 1, 7, NULL, 'Bài viết rất hữu ích! Mình đang cân nhắc chuyển từ canh tác truyền thống sang hữu cơ.', 2, 0, 'visible', '2025-09-09 07:00:00', '2025-09-09 07:00:00'),
(4, 2, 5, NULL, 'Cảm ơn bạn đã đánh giá sản phẩm của chúng tôi! Nếu có bất kỳ thắc mắc nào về sử dụng, hãy liên hệ với bộ phận hỗ trợ kỹ thuật nhé.', 4, 0, 'visible', '2025-09-08 17:00:00', '2025-09-08 17:00:00'),
(5, 2, 10, 4, 'Máy chạy rất ổn, chỉ có điều pin hơi nhanh hết khi làm đất cứng. Các bạn có kế hoạch nâng cấp dung lượng pin không?', 1, 0, 'visible', '2025-09-09 06:00:00', '2025-09-09 06:00:00'),
(6, 3, 9, NULL, 'Bạn có thể thử dùng dung dịch tỏi ớt để xịt phòng trừ sâu bệnh. Mình dùng hiệu quả lắm!', 6, 0, 'visible', '2025-09-09 10:00:00', '2025-09-09 10:00:00');

-- Insert Chatbot Conversations (v7.1 structure - changed user_id to customer_id)
INSERT INTO `chatbot_conversations` (`id`, `customer_id`, `session_id`, `title`, `context`, `is_active`, `started_at`, `ended_at`) VALUES
(1, 7, 'session_20250908_001', 'Tư vấn chọn máy cày', '{"topic": "equipment_consultation", "products_discussed": ["TC001"], "user_preferences": {"budget": "under_30m", "farm_size": "small"}}', 0, '2025-09-08 14:00:00', '2025-09-08 14:30:00'),
(2, 9, 'session_20250909_001', 'Hỗ trợ kỹ thuật canh tác', '{"topic": "farming_techniques", "crop_type": "rice", "farming_method": "organic"}', 1, '2025-09-09 06:00:00', NULL),
(3, 8, 'session_20250909_002', 'Tư vấn phân bón hữu cơ', '{"topic": "fertilizer_consultation", "crop_type": "vegetables", "soil_type": "sandy"}', 0, '2025-09-09 08:00:00', '2025-09-09 08:45:00');

-- Insert Chatbot Messages (v8.1 structure - removed attachments, use media_links table instead)
INSERT INTO `chatbot_messages` (`id`, `conversation_id`, `message_type`, `message_text`, `created_at`) VALUES
(1, 1, 'user', 'Tôi cần tư vấn chọn máy cày cho ruộng nhỏ khoảng 2ha', '2025-09-08 14:00:00'),
(2, 1, 'bot', 'Với diện tích 2ha, tôi khuyên bạn nên chọn máy cày mini điện VerdantTech V1. Máy này có những ưu điểm: tiết kiệm năng lượng, thân thiện môi trường, phù hợp với ruộng nhỏ.', '2025-09-08 14:00:30'),
(3, 1, 'user', 'Giá của máy này là bao nhiêu? Có khuyến mãi không?', '2025-09-08 14:02:00'),
(4, 1, 'bot', 'Máy cày mini điện VerdantTech V1 có giá 25.000.000 VNĐ. Hiện tại đang có khuyến mãi 10%, giá sau khuyến mãi là 22.500.000 VNĐ. Bạn có muốn xem thêm thông tin chi tiết không?', '2025-09-08 14:02:30'),
(5, 2, 'user', 'Tôi muốn hỏi về kỹ thuật trồng lúa hữu cơ', '2025-09-09 06:00:00'),
(6, 2, 'bot', 'Trồng lúa hữu cơ cần chú ý những điểm sau: 1) Chuẩn bị đất: Sử dụng phân compost thay vì phân hóa học 2) Chọn giống: Nên chọn giống lúa địa phương thích nghi 3) Quản lý nước: Tưới tiêu đúng thời điểm 4) Phòng trừ sâu bệnh: Sử dụng biện pháp sinh học', '2025-09-09 06:01:00'),
(7, 3, 'user', 'Tôi trồng rau, đất cát, nên dùng loại phân nào?', '2025-09-09 08:00:00'),
(8, 3, 'bot', 'Với đất cát trồng rau, tôi khuyên bạn sử dụng Phân Compost Hữu Cơ Premium. Loại phân này giúp cải thiện cấu trúc đất cát, tăng khả năng giữ nước và cung cấp dinh dưỡng lâu dài cho cây trồng.', '2025-09-09 08:00:30'),
(9, 3, 'user', 'Cảm ơn bạn! Tôi sẽ đặt mua ngay.', '2025-09-09 08:44:00'),
(10, 3, 'bot', 'Rất vui được hỗ trợ bạn! Chúc bạn canh tác thành công. Nếu có thắc mắc gì khác, đừng ngần ngại liên hệ nhé!', '2025-09-09 08:44:30');

-- Insert Environmental Data (v7.2 structure - updated column names and added new environmental measurement fields)
INSERT INTO `environmental_data` (`id`, `farm_profile_id`, `customer_id`, `measurement_start_date`, `measurement_end_date`, `sand_pct`, `silt_pct`, `clay_pct`, `phh2o`, `precipitation_sum`, `et0_fao_evapotranspiration`, `co2_footprint`, `notes`, `created_at`, `updated_at`) VALUES
(1, 1, 9, '2025-09-09', '2025-09-09', 35.50, 45.20, 19.30, 6.50, 85.40, 4.20, 120.50, 'Đo lường sau mưa', '2025-09-09 06:00:00', '2025-09-09 06:00:00'),
(2, 2, 10, '2025-09-09', '2025-09-09', 28.70, 41.80, 29.50, 7.00, 92.60, 4.80, 85.30, 'Kiểm tra hàng tuần', '2025-09-09 05:30:00', '2025-09-09 05:30:00');

-- Insert Fertilizers (added for completeness)
INSERT INTO `fertilizers` (`id`, `environmental_data_id`, `organic_fertilizer`, `npk_fertilizer`, `urea_fertilizer`, `phosphate_fertilizer`, `created_at`, `updated_at`) VALUES
(1, 1, 50.00, 10.00, 5.00, 15.00, '2025-09-09 06:00:00', '2025-09-09 06:00:00'),
(2, 2, 40.00, 8.00, 4.00, 12.00, '2025-09-09 05:30:00', '2025-09-09 05:30:00');

-- Insert Energy Usage (added for completeness)
INSERT INTO `energy_usage` (`id`, `environmental_data_id`, `electricity_kwh`, `gasoline_liters`, `diesel_liters`, `created_at`, `updated_at`) VALUES
(1, 1, 100.00, 20.00, 30.00, '2025-09-09 06:00:00', '2025-09-09 06:00:00'),
(2, 2, 80.00, 15.00, 25.00, '2025-09-09 05:30:00', '2025-09-09 05:30:00');

-- Insert Requests (v7.1 structure - removed amount, admin_notes, rejection_reason columns, changed requester_id to user_id)
INSERT INTO `requests` (`id`, `user_id`, `request_type`, `title`, `description`, `status`, `reply_notes`, `processed_by`, `processed_at`, `created_at`, `updated_at`) VALUES
(1, 5, 'support_request', 'Yêu cầu thanh toán hoa hồng tháng 9', 'Yêu cầu thanh toán hoa hồng từ bán hàng tháng 9', 'pending', NULL, NULL, NULL, '2025-09-09 07:00:00', '2025-09-09 07:00:00'),
(2, 7, 'refund_request', 'Yêu cầu hoàn tiền đơn hàng #1', 'Sản phẩm bị hỏng', 'in_review', 'Kiểm tra sản phẩm', NULL, NULL, '2025-09-09 08:15:00', '2025-09-09 08:15:00');

-- Insert Orders (v8.1 structure - added courier_id, width, height, length, weight)
INSERT INTO `orders` (`id`, `customer_id`, `status`, `subtotal`, `tax_amount`, `shipping_fee`, `discount_amount`, `total_amount`, `address_id`, `order_payment_method`, `shipping_method`, `tracking_number`, `notes`, `courier_id`, `width`, `height`, `length`, `weight`, `cancelled_reason`, `cancelled_at`, `confirmed_at`, `shipped_at`, `delivered_at`, `created_at`, `updated_at`) VALUES
(1, 7, 'delivered', 25000000.00, 500000.00, 300000.00, 2500000.00, 23000000.00, 6, 'Banking', 'express', 'EXP20250908001', NULL, 1, 120, 150, 250, 500000, NULL, NULL, '2025-09-08 12:00:00', '2025-09-08 15:00:00', '2025-09-09 10:00:00', '2025-09-08 10:00:00', '2025-09-09 10:00:00'),
(2, 8, 'paid', 1716750.00, 0.00, 50000.00, 383250.00, 1483500.00, 7, 'COD', 'standard', 'STD20250909001', NULL, 2, 80, 100, 150, 250000, NULL, NULL, '2025-09-09 10:00:00', NULL, NULL, '2025-09-09 09:00:00', '2025-09-09 10:00:00'),
(3, 9, 'processing', 12000000.00, 800000.00, 200000.00, 960000.00, 11240000.00, 4, 'Rent', 'express', NULL, 'Cần hỗ trợ lắp đặt', 1, 200, 150, 450, 2500000, NULL, NULL, NULL, NULL, NULL, '2025-09-09 11:00:00', '2025-09-09 11:30:00'),
(4, 10, 'paid', 8500000.00, 425000.00, 150000.00, 680000.00, 8395000.00, 5, 'Banking', 'standard', 'STD20250910001', 'Giao hàng trong giờ hành chính', 2, 100, 120, 200, 800000, NULL, NULL, '2025-09-10 09:00:00', NULL, NULL, '2025-09-10 08:00:00', '2025-09-10 09:00:00');

-- Insert Order Details
INSERT INTO `order_details` (`id`, `order_id`, `product_id`, `quantity`, `unit_price`, `discount_amount`, `subtotal`, `created_at`) VALUES
(1, 1, 1, 1, 25000000.00, 2500000.00, 22500000.00, '2025-09-08 10:00:00'),
(2, 2, 3, 10, 150000.00, 0.00, 1500000.00, '2025-09-09 09:00:00'),
(3, 2, 4, 3, 85000.00, 38250.00, 216750.00, '2025-09-09 09:00:00'),
(4, 3, 5, 1, 12000000.00, 960000.00, 11040000.00, '2025-09-09 11:00:00'),
(5, 4, 3, 50, 50000.00, 500000.00, 2500000.00, '2025-09-10 08:00:00'),
(6, 4, 4, 80, 90000.00, 180000.00, 7200000.00, '2025-09-10 08:00:00');

-- Insert Transactions (v7.1 structure - completely restructured)
INSERT INTO `transactions` (`id`, `transaction_type`, `amount`, `currency`, `order_id`, `user_id`, `status`, `note`, `gateway_payment_id`, `created_by`, `processed_by`, `created_at`, `completed_at`, `updated_at`) VALUES
(1, 'payment_in', 23000000.00, 'VND', 1, 7, 'completed', 'Payment for order #1 - Máy cày', 'VNP20250908001', 7, 1, '2025-09-08 11:30:00', '2025-09-08 11:30:00', '2025-09-08 11:30:00'),
(2, 'payment_in', 1483500.00, 'VND', 2, 8, 'completed', 'Payment for order #2 - Hạt giống và phân bón', 'MOMO20250909001', 8, 1, '2025-09-09 09:15:00', '2025-09-09 09:15:00', '2025-09-09 09:15:00'),
(3, 'payment_in', 11240000.00, 'VND', 3, 9, 'pending', 'Payment for order #3 - Drone phun thuốc', 'COD2025090911001', 9, NULL, '2025-09-09 11:00:00', NULL, '2025-09-09 11:00:00'),
(4, 'commission', 2000000.00, 'VND', 1, 5, 'completed', 'Commission from sale of product #1', NULL, 1, 2, '2025-09-09 15:00:00', '2025-09-09 15:00:00', '2025-09-09 15:00:00'),
(5, 'commission', 147000.00, 'VND', 2, 6, 'completed', 'Commission from sale of products #3 and #4', NULL, 1, 2, '2025-09-09 16:00:00', '2025-09-09 16:00:00', '2025-09-09 16:00:00');

-- Insert Payments (v7.1 structure - removed old columns, added gateway_payment_id)
INSERT INTO `payments` (`id`, `order_id`, `payment_method`, `payment_gateway`, `gateway_payment_id`, `amount`, `status`, `gateway_response`, `created_at`, `updated_at`) VALUES
(1, 1, 'payos', 'payos', 'PAYOS2025090801234567', 23000000.00, 'completed', '{"code": "00", "message": "Success", "bank": "VCB"}', '2025-09-08 10:00:00', '2025-09-08 11:30:00'),
(2, 2, 'credit_card', 'stripe', 'STR_2025090909876543', 1483500.00, 'completed', '{"id": "ch_abc123", "status": "succeeded"}', '2025-09-09 09:00:00', '2025-09-09 09:15:00'),
(3, 3, 'cod', 'manual', 'COD2025090911001', 11240000.00, 'pending', '{}', '2025-09-09 11:00:00', '2025-09-09 11:00:00');

-- Insert Cashouts (v7.1 structure - removed bank details, added bank_account_id and reason)
INSERT INTO `cashouts` (`id`, `vendor_id`, `transaction_id`, `bank_account_id`, `amount`, `status`, `reason`, `gateway_transaction_id`, `reference_type`, `reference_id`, `notes`, `processed_by`, `created_at`, `processed_at`, `updated_at`) VALUES
(1, 5, 4, 1, 2000000.00, 'pending', 'Commission payout', NULL, 'order', 1, 'Hoa hồng từ đơn hàng #1', NULL, '2025-09-09 15:30:00', NULL, '2025-09-09 15:30:00'),
(2, 6, 5, 2, 147000.00, 'completed', 'Commission payout', 'CASHOUT2025090916001', 'order', 2, 'Hoa hồng từ đơn hàng #2', 2, '2025-09-09 16:30:00', '2025-09-09 16:30:00', '2025-09-09 16:30:00');

-- Insert Batch Inventory (v7.1 structure - changed vendor_profile_id to vendor_id)
INSERT INTO `batch_inventory` (`id`, `product_id`, `sku`, `vendor_id`, `batch_number`, `lot_number`, `quantity`, `unit_cost_price`, `expiry_date`, `manufacturing_date`, `quality_check_status`, `quality_checked_by`, `quality_checked_at`, `notes`, `created_at`, `updated_at`) VALUES
(1, 1, 'SKU_TC001_001', 5, 'BATCH001', 'LOT001', 5, 18000000.00, NULL, '2025-08-01', 'passed', 2, '2025-09-08 10:00:00', 'Máy cày đầu tiên nhập kho', '2025-09-08 09:00:00', '2025-09-08 10:00:00'),
(2, 2, 'SKU_HV002_001', 5, 'BATCH002', 'LOT002', 10, 70000000.00, NULL, '2025-07-15', 'passed', 2, '2025-09-08 15:00:00', 'Máy gặt lúa nhập kho', '2025-09-08 14:00:00', '2025-09-09 16:00:00'),
(3, 3, 'SKU_SD003_001', 6, 'BATCH003', 'LOT003', 200, 25000.00, '2026-09-08', '2025-06-01', 'passed', 2, '2025-09-08 12:00:00', 'Hạt giống nhập kho', '2025-09-08 11:00:00', '2025-09-09 15:00:00'),
(4, 4, 'SKU_FT004_001', 6, 'BATCH004', 'LOT004', 100, 60000.00, '2026-03-01', '2025-05-01', 'passed', 2, '2025-09-08 09:00:00', 'Phân bón nhập kho', '2025-09-08 08:00:00', '2025-09-08 08:00:00'),
(5, 5, 'SKU_DR005_001', 5, 'BATCH005', 'LOT005', 15, 20000000.00, NULL, '2025-08-15', 'passed', 2, '2025-09-08 11:00:00', 'Drone phun thuốc nhập kho', '2025-09-08 10:00:00', '2025-09-09 09:00:00');

-- Insert Export Inventory (v7.1 structure - removed unit_sale_price column, added updated_at)
INSERT INTO `export_inventory` (`id`, `product_id`, `order_id`, `quantity`, `movement_type`, `notes`, `created_by`, `created_at`, `updated_at`) VALUES
(1, 1, 1, 1, 'sale', 'Máy cày bán cho khách hàng 1', 5, '2025-09-08 10:00:00', '2025-09-08 10:00:00'),
(2, 3, 2, 10, 'sale', 'Hạt giống bán cho khách hàng 2', 6, '2025-09-09 09:00:00', '2025-09-09 09:00:00'),
(3, 4, 2, 3, 'sale', 'Phân bón bán kèm hạt giống', 6, '2025-09-09 09:00:00', '2025-09-09 09:00:00'),
(4, 5, 3, 1, 'sale', 'Drone phun thuốc cho nông dân 1', 5, '2025-09-09 11:00:00', '2025-09-09 11:00:00');

-- Insert Product Reviews (v8.1 structure - removed images, use media_links table instead)
INSERT INTO `product_reviews` (`id`, `product_id`, `order_id`, `customer_id`, `rating`, `title`, `comment`, `created_at`, `updated_at`) VALUES
(1, 1, 1, 7, 5, 'Máy cày tuyệt vời!', 'Máy chạy êm, tiết kiệm điện và rất phù hợp với ruộng nhỏ của tôi. Chất lượng tốt, đóng gói cẩn thận.', '2025-09-09 16:00:00', '2025-09-09 16:00:00'),
(2, 3, 2, 8, 4, 'Hạt giống chất lượng', 'Hạt giống nảy mầm tốt, tỷ lệ cao như quảng cáo. Cây trồng phát triển khỏe mạnh.', '2025-09-09 18:00:00', '2025-09-09 18:00:00'),
(3, 4, 2, 8, 5, 'Phân compost chất lượng', 'Phân rất tốt, cây trồng phát triển nhanh sau khi bón. Mùi không quá nặng, dễ sử dụng.', '2025-09-09 19:00:00', '2025-09-09 19:00:00');

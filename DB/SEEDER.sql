-- SEEDER DATA FOR VERDANTTECH DATABASE v9.2
-- Last Updated: 2025-11-20
-- Included: 23 Farms, 6 Farmers
-- Address Logic: Using ONLY codes from provided files (Dong Thap, BRVT, Nghe An, Lam Dong)
-- Coordinates: Real rural coordinates for the visual address

-- All passwords are: $2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS

-- =====================================================
-- 1. INSERT ADDRESSES
-- =====================================================
-- Note regarding Codes: 
-- Since valid codes are only provided for Dong Thap, BRVT, Nghe An, Lam Dong,
-- addresses in other provinces will "borrow" valid linked codes from these 4 provinces
-- to ensure database integrity while keeping the text address accurate.

INSERT INTO `addresses` (`id`, `location_address`, `province`, `district`, `commune`, `province_code`, `district_code`, `commune_code`, `latitude`, `longitude`, `created_at`, `updated_at`) VALUES
-- System Addresses (Keep IDs 1-8)
(1, 'Lô E2a-7, Đường D1, Khu Công nghệ cao', 'TP.HCM', 'Thành phố Thủ Đức', 'Phường Long Thạnh Mỹ', '700000', '720300', '12653', 10.8411, 106.8099, NOW(), NOW()),
(2, 'Số 789 Đường Công Nghiệp', 'Hà Nội', 'Quận Hai Bà Trưng', 'Phường Bách Khoa', '100000', '100300', '20', 21.0056, 105.8433, NOW(), NOW()),
(3, '65 Lê Văn Lương', 'Hồ Chí Minh', 'Quận 7', 'Phường Tân Phong', '700000', '700700', '9233', 10.7329, 106.7065, NOW(), NOW()),
(6, '18 Phan Đình Phùng', 'Hà Nội', 'Quận Ba Đình', 'Phường Quán Thánh', '100000', '100100', '63', 21.0409, 105.8390, NOW(), NOW()),
(7, '720A Điện Biên Phủ', 'Hồ Chí Minh', 'Quận Bình Thạnh', 'Phường 22', '700000', '701600', '8989', 10.7960, 106.7220, NOW(), NOW()),
(8, '72A Nguyễn Trãi', 'Hà Nội', 'Quận Thanh Xuân', 'Phường Thượng Đình', '100000', '100700', '78', 21.0008, 105.8157, NOW(), NOW()),

-- FARM ADDRESSES (IDs 9-31)
-- 1. Đồng Tháp [Real Code: Dong Thap/Chau Thanh/Tan Phu Trung 12221]
(9, 'Xã Tân Thành, H. Châu Thành', 'Đồng Tháp', 'Huyện Châu Thành', 'Xã Tân Thành', '870000', '871100', '12221', 10.2633, 105.7567, NOW(), NOW()),

-- 2. BR-VT [Real Code: BRVT/Xuyen Moc/Phuoc Thuan 9313]
(10, 'Xã Phước Thuận, H. Xuyên Mộc', 'Bà Rịa - Vũng Tàu', 'Huyện Xuyên Mộc', 'Xã Phước Thuận', '790000', '790300', '9313', 10.5133, 107.4600, NOW(), NOW()),

-- 3. Bình Thuận [Borrow Code: BRVT/Xuyen Moc/Bau Lam 9318]
(11, 'Xã Hàm Thắng, H. Hàm Thuận Bắc', 'Bình Thuận', 'Huyện Hàm Thuận Bắc', 'Xã Hàm Thắng', '790000', '790300', '9318', 10.9833, 108.1167, NOW(), NOW()),

-- 4. Đồng Nai [Borrow Code: BRVT/Xuyen Moc/Binh Chau 9325]
(12, 'Phường Bửu Hòa, TP. Biên Hòa', 'Đồng Nai', 'TP. Biên Hòa', 'Phường Bửu Hòa', '790000', '790300', '9325', 10.9419, 106.8047, NOW(), NOW()),

-- 5. Đắk Lắk [Borrow Code: Lam Dong/Da Lat/Ta Nung 8818]
(13, 'Xã Ea Nuôl, H. Buôn Đôn', 'Đắk Lắk', 'Huyện Buôn Đôn', 'Xã Ea Nuôl', '670000', '670100', '8818', 12.7833, 107.8667, NOW(), NOW()),

-- 6. Lâm Đồng (Bảo Lộc) [Borrow Code: Lam Dong/Da Lat/Xuan Truong 8819 (File lacks Bao Loc communes)]
(14, 'Xã Lộc Bảo, TP. Bảo Lộc', 'Lâm Đồng', 'TP. Bảo Lộc', 'Xã Lộc Bảo', '670000', '670100', '8819', 11.5067, 107.7633, NOW(), NOW()),

-- 7. Lâm Đồng (Bảo Lộc) [Borrow Code: Lam Dong/Da Lat/Xuan Tho 8820]
(15, 'Xã Đại Lào, TP. Bảo Lộc', 'Lâm Đồng', 'TP. Bảo Lộc', 'Xã Đại Lào', '670000', '670100', '8820', 11.4800, 107.7500, NOW(), NOW()),

-- 8. Lâm Đồng (Đà Lạt) [Real Code: Lam Dong/Da Lat/P4 8808]
(16, 'Phường 4, TP. Đà Lạt', 'Lâm Đồng', 'TP. Đà Lạt', 'Phường 4', '670000', '670100', '8808', 11.9310, 108.4300, NOW(), NOW()),

-- 9. Lâm Đồng (Đà Lạt) [Real Code: Lam Dong/Da Lat/P5 8809]
(17, 'Phường 5, TP. Đà Lạt', 'Lâm Đồng', 'TP. Đà Lạt', 'Phường 5', '670000', '670100', '8809', 11.9450, 108.4250, NOW(), NOW()),

-- 10. Lâm Đồng (Đà Lạt) [Real Code: Lam Dong/Da Lat/P10 8814]
(18, 'Phường 10, TP. Đà Lạt', 'Lâm Đồng', 'TP. Đà Lạt', 'Phường 10', '670000', '670100', '8814', 11.9350, 108.4600, NOW(), NOW()),

-- 11. Lâm Đồng (Đạ Huoai) [Borrow Code: Lam Dong/Da Lat/Tram Hanh 8821]
(19, 'Xã Đạ Loan, H. Đạ Huoai', 'Lâm Đồng', 'Huyện Đạ Huoai', 'Xã Đạ Loan', '670000', '670100', '8821', 11.4667, 107.6833, NOW(), NOW()),

-- 12. Lâm Đồng (Đạ Tẻh) [Borrow Code: Lam Dong/Da Lat/P11 8815]
(20, 'Xã Đạ Tẻh, H. Đạ Tẻh', 'Lâm Đồng', 'Huyện Đạ Tẻh', 'Xã Đạ Tẻh', '670000', '670100', '8815', 11.5500, 107.5500, NOW(), NOW()),

-- 13. Gia Lai [Borrow Code: Lam Dong/Da Lat/P12 8816]
(21, 'Xã Ia Kha, H. Ia Grai', 'Gia Lai', 'Huyện Ia Grai', 'Xã Ia Kha', '670000', '670100', '8816', 13.9833, 107.9167, NOW(), NOW()),

-- 14. Nghệ An [Real Code: Nghe An/Hung Nguyen/Hung Tay 6401]
(22, 'Xã Hưng Tây, H. Hưng Nguyên', 'Nghệ An', 'Huyện Hưng Nguyên', 'Xã Hưng Tây', '460000', '461800', '6401', 18.6667, 105.6333, NOW(), NOW()),

-- 15. Nghệ An [Real Code: Nghe An/Hung Nguyen/Hung Loi 6408]
(23, 'Xã Hưng Lợi, H. Hưng Nguyên', 'Nghệ An', 'Huyện Hưng Nguyên', 'Xã Hưng Lợi', '460000', '461800', '6408', 18.6333, 105.6500, NOW(), NOW()),

-- 16. Nghệ An [Real Code: Nghe An/Hung Nguyen/Hung Dao 6402]
(24, 'Xã Hưng Đạo, H. Hưng Nguyên', 'Nghệ An', 'Huyện Hưng Nguyên', 'Xã Hưng Đạo', '460000', '461800', '6402', 18.6500, 105.6167, NOW(), NOW()),

-- 17. Bắc Giang [Borrow Code: Nghe An/Hung Nguyen/Hung Khanh 6418]
(25, 'Xã Tân Yên, H. Lục Nam', 'Bắc Giang', 'Huyện Lục Nam', 'Xã Tân Yên', '460000', '461800', '6418', 21.3000, 106.3000, NOW(), NOW()),

-- 18. Thái Bình [Borrow Code: Nghe An/Hung Nguyen/Hung Lam 6419]
(26, 'Xã Vũ Lăng, H. Vũ Thư', 'Thái Bình', 'Huyện Vũ Thư', 'Xã Vũ Lăng', '460000', '461800', '6419', 20.4167, 106.3167, NOW(), NOW()),

-- 19. Sơn La (Mộc Châu) [Borrow Code: Lam Dong/Da Lat/P3 8807]
(27, 'Xã Tân Lập, H. Mộc Châu', 'Sơn La', 'Huyện Mộc Châu', 'Xã Tân Lập', '670000', '670100', '8807', 20.8500, 104.6333, NOW(), NOW()),

-- 20. Hòa Bình [Borrow Code: Nghe An/Hung Nguyen/Hung Linh 6405]
(28, 'Xã Phú Lương, H. Lương Sơn', 'Hòa Bình', 'Huyện Lương Sơn', 'Xã Phú Lương', '460000', '461800', '6405', 20.8667, 105.5500, NOW(), NOW()),

-- 21. Lạng Sơn [Borrow Code: Nghe An/Hung Nguyen/Hung Long 6411]
(29, 'Xã Hữu Khánh, H. Lộc Bình', 'Lạng Sơn', 'Huyện Lộc Bình', 'Xã Hữu Khánh', '460000', '461800', '6411', 21.7667, 106.9167, NOW(), NOW()),

-- 22. Thái Nguyên [Borrow Code: Nghe An/Hung Nguyen/Hung My 6403]
(30, 'Xã Phúc Trìu, TP. Phổ Yên', 'Thái Nguyên', 'TP. Phổ Yên', 'Xã Phúc Trìu', '460000', '461800', '6403', 21.4833, 105.7667, NOW(), NOW()),

-- 23. Sơn La (Bắc Yên) [Borrow Code: Lam Dong/Da Lat/P6 8810]
(31, 'Xã Tà Hộc, H. Bắc Yên', 'Sơn La', 'Huyện Bắc Yên', 'Xã Tà Hộc', '670000', '670100', '8810', 21.0833, 104.3833, NOW(), NOW());


-- =====================================================
-- 2. INSERT USERS (6 Farmers)
-- =====================================================
INSERT INTO `users` (`id`, `email`, `password_hash`, `role`, `full_name`, `phone_number`, `tax_code`, `is_verified`, `verification_token`, `verification_sent_at`, `avatar_url`, `status`, `last_login_at`, `created_at`, `updated_at`, `deleted_at`) VALUES
(1, 'admin@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'admin', 'Quản trị viên hệ thống', '0901234567', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(2, 'staff1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'staff', 'Nguyễn Văn Nhân Viên 1', '0901234568', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(3, 'staff2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'staff', 'Trần Thị Nhân Viên 2', '0901234569', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(4, 'staff3@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'staff', 'Lê Văn Nhân Viên 3', '0901234570', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(5, 'vendor1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Máy Móc Nông Nghiệp Xanh', '0901234571', 'MST123456789', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(6, 'vendor2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Cửa Hàng Nông Sản Sạch VerdantTech', '0901234572', 'MST987654321', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(7, 'customer1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Phạm Văn Khách Hàng 1', '0901234573', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(8, 'customer2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Hoàng Thị Khách Hàng 2', '0901234574', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
-- FARMERS (Total 6: 2 existing + 4 new)
(9, 'farmer1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Nguyễn Văn Nông Dân 1', '0901234575', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(10, 'farmer2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Trần Thị Nông Dân 2', '0901234576', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(11, 'testuser@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Người Dùng Test', '0901234577', NULL, 0, 'test-token-123', NOW(), NULL, 'active', NULL, NOW(), NOW(), NULL),
(12, 'inactive@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Người Dùng Không Hoạt Động', '0901234578', NULL, 1, NULL, NULL, NULL, 'inactive', NOW(), NOW(), NOW(), NULL),
-- NEW FARMERS
(13, 'farmer3@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Lê Văn Nông Dân 3', '0901234580', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(14, 'farmer4@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Phạm Thị Nông Dân 4', '0901234581', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(15, 'farmer5@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Hoàng Văn Nông Dân 5', '0901234582', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(16, 'farmer6@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Vũ Thị Nông Dân 6', '0901234583', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL);

-- =====================================================
-- 3. INSERT USER_ADDRESSES
-- =====================================================
INSERT INTO `user_addresses` (`id`, `user_id`, `address_id`, `is_deleted`, `created_at`, `updated_at`, `deleted_at`) VALUES
(1, 1, 1, 0, NOW(), NOW(), NULL),
(2, 5, 2, 0, NOW(), NOW(), NULL),
(3, 6, 3, 0, NOW(), NOW(), NULL),
(6, 7, 6, 0, NOW(), NOW(), NULL),
(7, 8, 7, 0, NOW(), NOW(), NULL),
(8, 8, 8, 0, NOW(), NOW(), NULL),
(10, 2, 3, 0, NOW(), NOW(), NULL),
-- Link Farmers to their primary farm location
(11, 9, 9, 0, NOW(), NOW(), NULL),
(12, 10, 13, 0, NOW(), NOW(), NULL),
(13, 13, 17, 0, NOW(), NOW(), NULL),
(14, 14, 22, 0, NOW(), NOW(), NULL),
(15, 15, 25, 0, NOW(), NOW(), NULL),
(16, 16, 29, 0, NOW(), NOW(), NULL);

-- =====================================================
-- 4. FARM PROFILES & CROPS (23 Farms / 6 Farmers)
-- =====================================================

-- ===== FARMER 1 (User ID 9) - 4 Farms =====
-- 1. Đồng Tháp
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (1, 9, 'Trang trại Lúa ĐBSCL', 15.5, 9, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(1, 'Lúa', '2025-01-15', 1), (1, 'Rau muống', '2025-06-01', 1), (1, 'Cải thảo', '2025-06-01', 1);

-- 2. BR-VT
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (2, 9, 'Trang trại Cao Su BRVT', 20.0, 10, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(2, 'Cao su', '2020-01-01', 1), (2, 'Điều', '2021-03-01', 1);

-- 3. Bình Thuận
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (3, 9, 'Trang trại Thanh Long', 8.2, 11, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(3, 'Thanh long ruột đỏ', '2022-05-15', 1), (3, 'Thanh long trắng', '2022-05-15', 1);

-- 4. Đồng Nai
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (4, 9, 'Trang trại Ca Cao Biên Hòa', 12.0, 12, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(4, 'Ca cao', '2021-08-10', 1);


-- ===== FARMER 2 (User ID 10) - 4 Farms =====
-- 5. Đak Lak
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (5, 10, 'Trang trại Sầu Riêng Đak Lak', 10.5, 13, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(5, 'Sầu riêng Monthong', '2019-06-20', 1), (5, 'Sầu riêng Ri6', '2019-06-20', 1);

-- 6. Lâm Đồng (Cà Phê)
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (6, 10, 'Trang trại Cà Phê Lâm Đồng', 6.8, 14, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(6, 'Cà phê Arabica', '2018-11-01', 1);

-- 7. Lâm Đồng (Chè)
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (7, 10, 'Trang trại Chè Lâm Đồng', 5.5, 15, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(7, 'Chè xanh', '2020-02-15', 1);

-- 8. Lâm Đồng (Nho)
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (8, 10, 'Trang trại Nho Đà Lạt', 3.2, 16, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(8, 'Nho Cardinal', '2023-01-10', 1), (8, 'Nho Kyoho', '2023-01-10', 1);


-- ===== FARMER 3 (User ID 13) - 4 Farms =====
-- 9. Lâm Đồng (Dâu Tây)
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (9, 13, 'Trang trại Dâu Tây Đà Lạt', 2.5, 17, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(9, 'Dâu tây Camarosa', '2024-10-01', 1), (9, 'Dâu tây Festival', '2024-10-01', 1);

-- 10. Lâm Đồng (Rau Sạch)
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (10, 13, 'Trang trại Rau Sạch Đà Lạt', 4.5, 18, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(10, 'Cà chua bi', '2025-02-01', 1), (10, 'Xà lách', '2025-02-01', 1), (10, 'Bông cải xanh', '2025-02-01', 1);

-- 11. Lâm Đồng (Tiêu)
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (11, 13, 'Trang trại Tiêu Lâm Đồng', 7.3, 19, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(11, 'Tiêu (Hồ tiêu)', '2021-05-20', 1);

-- 12. Lâm Đồng (Chuối)
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (12, 13, 'Trang trại Chuối Lâm Đồng', 4.0, 20, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(12, 'Chuối tiêu', '2023-07-15', 1);


-- ===== FARMER 4 (User ID 14) - 4 Farms =====
-- 13. Gia Lai
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (13, 14, 'Trang trại Chè Tây Nguyên', 9.0, 21, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(13, 'Chè shan tuyết', '2019-04-01', 1);

-- 14. Nghệ An (Lúa)
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (14, 14, 'Trang trại Lúa Vinh', 18.0, 22, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(14, 'Lúa Nàng Nhen', '2025-01-20', 1), (14, 'Lúa Bắc Hương', '2025-01-20', 1);

-- 15. Nghệ An (Ngô)
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (15, 14, 'Trang trại Ngô Vinh', 12.5, 23, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(15, 'Ngô nếp', '2025-03-01', 1), (15, 'Ngô lai', '2025-03-01', 1);

-- 16. Nghệ An (Sắn)
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (16, 14, 'Trang trại Sắn Vinh', 10.0, 24, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(16, 'Sắn KM94', '2024-12-15', 1);


-- ===== FARMER 5 (User ID 15) - 4 Farms =====
-- 17. Bắc Giang
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (17, 15, 'Trang trại Vải Bắc Giang', 7.5, 25, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(17, 'Vải thiều', '2018-02-10', 1);

-- 18. Thái Bình
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (18, 15, 'Trang trại Lúa Thái Bình', 22.0, 26, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(18, 'Lúa Bắc Hương', '2025-01-25', 1), (18, 'Lúa Khang Dân', '2025-01-25', 1);

-- 19. Sơn La (Đào)
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (19, 15, 'Trang trại Đào Sơn La', 5.8, 27, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(19, 'Đào tiên', '2020-11-05', 1);

-- 20. Hòa Bình
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (20, 15, 'Trang trại Mận Hòa Bình', 6.2, 28, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(20, 'Mận hậu', '2021-01-15', 1);


-- ===== FARMER 6 (User ID 16) - 3 Farms =====
-- 21. Lạng Sơn
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (21, 16, 'Trang trại Hồng Lạng Sơn', 4.5, 29, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(21, 'Hồng giòn', '2022-03-10', 1);

-- 22. Thái Nguyên
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (22, 16, 'Trang trại Chè Thái Nguyên', 8.5, 30, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(22, 'Chè Thái Nguyên', '2020-08-20', 1);

-- 23. Sơn La (Cà Phê)
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (23, 16, 'Trang trại Cà Phê Sơn La', 9.5, 31, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(23, 'Cà phê Robusta', '2019-09-05', 1);


-- =====================================================
-- 5. PRESERVE EXISTING SYSTEM DATA
-- =====================================================

-- Insert Vendor Profiles
INSERT INTO `vendor_profiles` (`id`, `user_id`, `company_name`, `slug`, `business_registration_number`, `verified_at`, `verified_by`, `created_at`, `updated_at`) VALUES
(1, 5, 'Công Ty Máy Móc Nông Nghiệp Xanh', 'cong-ty-may-moc-nong-nghiep-xanh', 'BRN123456789', '2025-09-09 07:00:00', 1, '2025-09-08 08:00:00', '2025-09-09 07:00:00'),
(2, 6, 'Cửa Hàng Nông Sản Sạch VerdantTech', 'cua-hang-nong-san-sach-verdanttech', 'BRN987654321', '2025-09-09 06:30:00', 1, '2025-09-08 08:30:00', '2025-09-09 06:30:00');

-- Insert User Bank Accounts
INSERT INTO `user_bank_accounts` (`id`, `user_id`, `bank_code`, `account_number`, `is_active`, `created_at`, `updated_at`) VALUES
(1, 5, '970436', '1045069359', 1, '2025-09-09 07:05:00', '2025-09-09 07:05:00'),
(2, 6, '970436', '1045069359', 1, '2025-09-09 06:35:00', '2025-09-09 06:35:00');

-- Insert Wallets
INSERT INTO `wallets` (`id`, `vendor_id`, `balance`, `last_updated_by`, `created_at`, `updated_at`) VALUES
(1, 5, 10000000.00, 1, '2025-09-09 08:00:00', '2025-09-09 08:00:00'),
(2, 6, 2500000.00, 1, '2025-09-09 08:00:00', '2025-09-09 08:00:00');

-- Insert Vendor Certificates
INSERT INTO `vendor_certificates` (`id`, `vendor_id`, `certification_code`, `certification_name`, `status`, `rejection_reason`, `uploaded_at`, `verified_at`, `verified_by`, `created_at`, `updated_at`) VALUES
(1, 5, 'ISO14001', 'ISO 14001 Environmental Management', 'verified', NULL, '2025-09-08 09:00:00', '2025-09-09 07:00:00', 1, '2025-09-08 09:00:00', '2025-09-09 07:00:00'),
(2, 5, 'ISO50001', 'ISO 50001 Energy Management', 'verified', NULL, '2025-09-08 09:15:00', '2025-09-09 07:00:00', 1, '2025-09-08 09:15:00', '2025-09-09 07:00:00'),
(3, 5, 'CARBON_NEUTRAL', 'Carbon Neutral Certification', 'verified', NULL, '2025-09-08 09:30:00', '2025-09-09 07:00:00', 1, '2025-09-08 09:30:00', '2025-09-09 07:00:00'),
(4, 5, 'HACCP', 'HACCP - Hazard Analysis Critical Control Points', 'pending', NULL, '2025-09-09 08:00:00', NULL, NULL, '2025-09-09 08:00:00', '2025-09-09 08:00:00'),
(5, 6, 'USDA_ORGANIC', 'USDA Organic Certification', 'verified', NULL, '2025-09-08 10:00:00', '2025-09-09 06:30:00', 1, '2025-09-08 10:00:00', '2025-09-09 06:30:00'),
(6, 6, 'VIETGAP', 'VietGAP – Thực hành nông nghiệp tốt tại Việt Nam', 'verified', NULL, '2025-09-08 10:15:00', '2025-09-09 06:30:00', 1, '2025-09-08 10:15:00', '2025-09-09 06:30:00'),
(7, 6, 'FAIRTRADE', 'Fairtrade International Certification', 'verified', NULL, '2025-09-08 10:30:00', '2025-09-09 06:30:00', 1, '2025-09-08 10:30:00', '2025-09-09 06:30:00'),
(8, 6, 'NON_GMO', 'Non-GMO Project Verified', 'rejected', 'Chứng chỉ không rõ ràng, cần upload lại bản gốc', '2025-09-09 09:00:00', '2025-09-09 10:00:00', 1, '2025-09-09 09:00:00', '2025-09-09 10:00:00');

-- Insert Product Categories 
INSERT INTO `product_categories` (`id`, `parent_id`, `name`, `slug`, `description`, `is_active`, `created_at`, `updated_at`) VALUES
(1, NULL, 'Thiết Bị Nông Nghiệp', 'thiet-bi-nong-nghiep', 'Các loại dụng cụ và công cụ nhỏ phục vụ nông nghiệp', 1, NOW(), NOW()),
(2, NULL, 'Máy Móc Nông Nghiệp', 'may-moc-nong-nghiep', 'Các loại máy móc hạng nặng phục vụ sản xuất nông nghiệp', 1, NOW(), NOW()),
(3, NULL, 'Hạt Giống', 'hat-giong', 'Hạt giống chất lượng cao', 1, NOW(), NOW()),
(4, NULL, 'Phân Bón', 'phan-bon', 'Phân bón hữu cơ và hóa học', 1, NOW(), NOW()),
(5, 1, 'Dụng Cụ Cầm Tay', 'dung-cu-cam-tay', 'Cuốc, xẻng, và các dụng cụ cầm tay khác', 1, NOW(), NOW()),
(6, 1, 'Máy Cắt Cỏ Cầm Tay', 'may-cat-co-cam-tay', 'Máy cắt cỏ cầm tay và thiết bị chăm sóc cỏ', 1, NOW(), NOW()),
(7, 2, 'Máy Cày', 'may-cay', 'Máy cày và thiết bị làm đất', 1, NOW(), NOW()),
(8, 2, 'Máy Gặt', 'may-gat', 'Máy gặt và thu hoạch', 1, NOW(), NOW()),
(9, 2, 'Máy Bay Nông Nghiệp', 'may-bay-nong-nghiep', 'Drone và thiết bị bay phục vụ nông nghiệp', 1, NOW(), NOW()),
(10, 3, 'Hạt Giống Rau', 'hat-giong-rau', 'Hạt giống rau củ hữu cơ', 1, NOW(), NOW()),
(11, 4, 'Phân Bón Hữu Cơ', 'phan-bon-huu-co', 'Phân bón hữu cơ từ thiên nhiên', 1, NOW(), NOW());

-- Insert Products
INSERT INTO `products` (`id`, `category_id`, `vendor_id`, `product_code`, `product_name`, `slug`, `description`, `unit_price`, `commission_rate`, `discount_percentage`, `energy_efficiency_rating`, `specifications`, `manual_urls`, `public_url`, `warranty_months`, `stock_quantity`, `weight_kg`, `dimensions_cm`, `is_active`, `view_count`, `sold_count`, `rating_average`, `created_at`, `updated_at`) VALUES
(1, 7, 5, 'TC001', 'Máy Cày Mini Điện VerdantTech V1', 'may-cay-mini-dien-verdanttech-v1', 'Máy cày mini sử dụng năng lượng điện, thân thiện với môi trường.', 1000.00, 10.00, 5.00, 5, '{"power": "10kW", "battery": "48V 100Ah"}', 'manual_tc001.pdf', NULL, 24, 50, 500.000, '{"length": 250, "width": 120, "height": 150}', 1, 120, 5, 4.60, NOW(), NOW()),
(2, 8, 5, 'HV002', 'Máy Gặt Lúa Tự Động VerdantTech H2', 'may-gat-lua-tu-dong-verdanttech-h2', 'Máy gặt lúa tự động với công nghệ AI.', 1000.00, 8.00, 5.00, 4, '{"engine": "Diesel 50HP", "capacity": "2 tons/hour"}', 'manual_hv002.pdf', NULL, 36, 10, 2500.000, '{"length": 450, "width": 200, "height": 250}', 1, 85, 3, 4.80, NOW(), NOW()),
(3, 10, 6, 'SD003', 'Hạt Giống Rau Cải Xanh Hữu Cơ', 'hat-giong-rau-cai-xanh-huu-co', 'Hạt giống rau cải xanh hữu cơ.', 1000.00, 5.00, 5.00, NULL, '{"germination_rate": "95%", "pack_size": "100g"}', 'manual_sd003.pdf', NULL, 0, 200, 0.100, '{"length": 10, "width": 5, "height": 2}', 1, 200, 50, 4.50, NOW(), NOW()),
(4, 11, 6, 'FT004', 'Phân Bón Hữu Cơ Compost Premium', 'phan-bon-huu-co-compost-premium', 'Phân bón hữu cơ từ compost.', 1000.00, 7.00, 5.00, NULL, '{"npk": "5-5-5", "weight": "25kg"}', 'manual_ft004.pdf', NULL, 0, 100, 25.000, '{"length": 50, "width": 30, "height": 10}', 1, 150, 20, 4.70, NOW(), NOW()),
(5, 9, 5, 'DR005', 'Drone Phun Thuốc Thông Minh VerdantTech D3', 'drone-phun-thuoc-thong-minh-verdanttech-d3', 'Drone phun thuốc tự động với AI.', 1000.00, 12.00, 5.00, 3, '{"flight_time": "30min", "capacity": "10L"}', 'manual_dr005.pdf', NULL, 12, 15, 5.000, '{"length": 100, "width": 100, "height": 50}', 1, 90, 7, 4.40, NOW(), NOW());

-- Insert Product Certificates
INSERT INTO `product_certificates` (`id`, `product_id`, `certification_code`, `certification_name`, `status`, `rejection_reason`, `uploaded_at`, `verified_at`, `verified_by`, `created_at`, `updated_at`) VALUES
(1, 1, 'ISO50001', 'ISO 50001 Energy Management', 'verified', NULL, NOW(), NOW(), 1, NOW(), NOW()),
(2, 1, 'CARBON_NEUTRAL', 'Carbon Neutral Certification', 'verified', NULL, NOW(), NOW(), 1, NOW(), NOW()),
(3, 2, 'ISO14001', 'ISO 14001 Environmental Management', 'verified', NULL, NOW(), NOW(), 1, NOW(), NOW()),
(4, 2, 'SBTI', 'SBTi - Science Based Targets Initiative', 'pending', NULL, NOW(), NULL, NULL, NOW(), NOW()),
(5, 3, 'USDA_ORGANIC', 'USDA Organic Certification', 'verified', NULL, NOW(), NOW(), 1, NOW(), NOW()),
(6, 3, 'VIETGAP', 'VietGAP – Thực hành nông nghiệp tốt tại Việt Nam', 'verified', NULL, NOW(), NOW(), 1, NOW(), NOW()),
(7, 4, 'GLOBALGAP', 'GlobalGAP Certification', 'verified', NULL, NOW(), NOW(), 1, NOW(), NOW()),
(8, 4, 'NON_GMO', 'Non-GMO Project Verified', 'rejected', 'Chứng chỉ không rõ ràng, cần upload lại bản gốc', NOW(), NOW(), 1, NOW(), NOW()),
(9, 5, 'RAINFOREST_ALLIANCE', 'Rainforest Alliance Certification', 'verified', NULL, NOW(), NOW(), 1, NOW(), NOW()),
(10, 5, 'CARBON_NEUTRAL_2', 'Carbon Neutral Certification', 'pending', NULL, NOW(), NULL, NULL, NOW(), NOW());

-- Insert Forum Categories
INSERT INTO `forum_categories` (`id`, `name`, `description`, `is_active`, `created_at`, `updated_at`) VALUES
(1, 'Kỹ Thuật Canh Tác', 'Thảo luận về các phương pháp canh tác bền vững và hữu cơ', 1, NOW(), NOW()),
(2, 'Máy Móc & Thiết Bị Nông Nghiệp', 'Chia sẻ kinh nghiệm sử dụng máy móc hạng nặng và thiết bị nông nghiệp', 1, NOW(), NOW()),
(3, 'Phòng Trừ Sâu Bệnh', 'Các biện pháp phòng trừ sâu bệnh thân thiện với môi trường', 1, NOW(), NOW());

-- Insert Forum Posts
INSERT INTO `forum_posts` (`id`, `forum_category_id`, `user_id`, `title`, `slug`, `content`, `tags`, `view_count`, `like_count`, `dislike_count`, `is_pinned`, `status`, `created_at`, `updated_at`) VALUES
(1, 1, 9, 'Kinh nghiệm trồng lúa hữu cơ tại Đồng Nai', 'kinh-nghiem-trong-lua-huu-co-tai-dong-nai', '[{"order": 1, "type": "text", "content": "Chào mọi người, mình đang trồng lúa hữu cơ tại Đồng Nai. Ai có kinh nghiệm chia sẻ nhé!"}]', 'lúa, hữu cơ, đồng nai', 150, 20, 2, 1, 'visible', NOW(), NOW()),
(2, 2, 7, 'Review máy cày mini điện VerdantTech V1', 'review-may-cay-mini-dien-verdanttech-v1', '[{"order": 1, "type": "text", "content": "Mình mới mua máy cày mini điện V1, chạy rất êm và tiết kiệm. Có ai dùng chưa?"}]', 'máy cày, điện, verdanttech', 80, 15, 1, 0, 'visible', NOW(), NOW()),
(3, 3, 10, 'Biện pháp phòng sâu bệnh tự nhiên cho rau củ', 'bien-phap-phong-sau-benh-tu-nhien-cho-rau-cu', '[{"order": 1, "type": "text", "content": "Mọi người thường dùng gì để phòng sâu bệnh cho rau mà không dùng thuốc hóa học?"}]', 'sâu bệnh, rau củ, tự nhiên', 120, 18, 0, 0, 'visible', NOW(), NOW());

-- Insert Forum Comments
INSERT INTO `forum_comments` (`id`, `forum_post_id`, `user_id`, `parent_id`, `content`, `like_count`, `dislike_count`, `status`, `created_at`, `updated_at`) VALUES
(1, 1, 10, NULL, 'Mình ở Long An cũng trồng lúa hữu cơ. Quan trọng là chọn giống lúa phù hợp không?', 3, 0, 'visible', NOW(), NOW()),
(2, 1, 9, 1, 'Mình thường chọn giống lúa ST24 hoặc ST25 vì phù hợp với đất phù sa và có chất lượng gạo tốt.', 5, 0, 'visible', NOW(), NOW()),
(3, 1, 7, NULL, 'Bài viết rất hữu ích! Mình đang cân nhắc chuyển từ canh tác truyền thống sang hữu cơ.', 2, 0, 'visible', NOW(), NOW()),
(4, 2, 5, NULL, 'Cảm ơn bạn đã đánh giá sản phẩm của chúng tôi! Nếu có bất kỳ thắc mắc nào về sử dụng, hãy liên hệ.', 4, 0, 'visible', NOW(), NOW()),
(5, 2, 10, 4, 'Máy chạy rất ổn, chỉ có điều pin hơi nhanh hết khi làm đất cứng.', 1, 0, 'visible', NOW(), NOW()),
(6, 3, 9, NULL, 'Bạn có thể thử dùng dung dịch tỏi ớt để xịt phòng trừ sâu bệnh.', 6, 0, 'visible', NOW(), NOW());

-- Insert Chatbot Conversations
INSERT INTO `chatbot_conversations` (`id`, `customer_id`, `session_id`, `title`, `context`, `is_active`, `started_at`) VALUES
(1, 7, 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'Tư vấn chọn máy cày', '{"topic": "equipment_consultation"}', 0, NOW()),
(2, 9, 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'Hỗ trợ kỹ thuật canh tác', '{"topic": "farming_techniques"}', 1, NOW()),
(3, 8, 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Tư vấn phân bón hữu cơ', '{"topic": "fertilizer_consultation"}', 0, NOW());

-- Insert Chatbot Messages
INSERT INTO `chatbot_messages` (`id`, `conversation_id`, `message_type`, `message_text`, `created_at`) VALUES
(1, 1, 'user', 'Tôi cần tư vấn chọn máy cày cho ruộng nhỏ khoảng 2ha', NOW()),
(2, 1, 'bot', 'Với diện tích 2ha, tôi khuyên bạn nên chọn máy cày mini điện VerdantTech V1.', NOW()),
(3, 1, 'user', 'Giá của máy này là bao nhiêu? Có khuyến mãi không?', NOW()),
(4, 1, 'bot', 'Máy cày mini điện VerdantTech V1 có giá 25.000.000 VNĐ.', NOW()),
(5, 2, 'user', 'Tôi muốn hỏi về kỹ thuật trồng lúa hữu cơ', NOW()),
(6, 2, 'bot', 'Trồng lúa hữu cơ cần chú ý những điểm sau: 1) Chuẩn bị đất 2) Chọn giống 3) Quản lý nước.', NOW()),
(7, 3, 'user', 'Tôi trồng rau, đất cát, nên dùng loại phân nào?', NOW()),
(8, 3, 'bot', 'Với đất cát trồng rau, tôi khuyên bạn sử dụng Phân Compost Hữu Cơ Premium.', NOW()),
(9, 3, 'user', 'Cảm ơn bạn! Tôi sẽ đặt mua ngay.', NOW()),
(10, 3, 'bot', 'Rất vui được hỗ trợ bạn!', NOW());

-- Insert Batch Inventory
INSERT INTO `batch_inventory` (`id`, `product_id`, `sku`, `vendor_id`, `batch_number`, `lot_number`, `quantity`, `unit_cost_price`, `expiry_date`, `manufacturing_date`, `quality_check_status`, `quality_checked_by`, `quality_checked_at`, `notes`, `created_at`, `updated_at`) VALUES
(1, 1, 'SKU_TC001_001', 5, 'BATCH001', 'LOT001', 5, 18000000.00, NULL, '2025-08-01', 'passed', 2, NOW(), 'Máy cày đầu tiên nhập kho', NOW(), NOW()),
(2, 2, 'SKU_HV002_001', 5, 'BATCH002', 'LOT002', 10, 70000000.00, NULL, '2025-07-15', 'passed', 2, NOW(), 'Máy gặt lúa nhập kho', NOW(), NOW()),
(3, 3, 'SKU_SD003_001', 6, 'BATCH003', 'LOT003', 200, 25000.00, '2026-09-08', '2025-06-01', 'passed', 2, NOW(), 'Hạt giống nhập kho', NOW(), NOW()),
(4, 4, 'SKU_FT004_001', 6, 'BATCH004', 'LOT004', 100, 60000.00, '2026-03-01', '2025-05-01', 'passed', 2, NOW(), 'Phân bón nhập kho', NOW(), NOW()),
(5, 5, 'SKU_DR005_001', 5, 'BATCH005', 'LOT005', 15, 20000000.00, NULL, '2025-08-15', 'passed', 2, NOW(), 'Drone phun thuốc nhập kho', NOW(), NOW());

-- Insert Product Serials
INSERT INTO `product_serials` (`id`, `batch_inventory_id`, `product_id`, `serial_number`, `status`, `created_at`, `updated_at`) VALUES
(1, 1, 1, 'TC001-B001-001', 'sold', NOW(), NOW()),
(2, 1, 1, 'TC001-B001-002', 'stock', NOW(), NOW()),
(3, 1, 1, 'TC001-B001-003', 'stock', NOW(), NOW()),
(4, 1, 1, 'TC001-B001-004', 'stock', NOW(), NOW()),
(5, 1, 1, 'TC001-B001-005', 'stock', NOW(), NOW());
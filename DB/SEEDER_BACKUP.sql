-- SEEDER DATA FOR VERDANTTECH DATABASE v9.2
-- Last Updated: 2025-11-20
-- Included: 23 Farms, 6 Farmers, 20 Vendors, 172 Products
-- Address Logic: Using ONLY codes from provided files (Dong Thap, BRVT, Nghe An, Lam Dong)
-- All passwords are: $2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS

-- =====================================================
-- 1. INSERT ADDRESSES (Keep existing farm addresses)
-- =====================================================
INSERT INTO `addresses` (`id`, `location_address`, `province`, `district`, `commune`, `province_code`, `district_code`, `commune_code`, `latitude`, `longitude`, `created_at`, `updated_at`) VALUES
-- System Addresses (Keep IDs 1-8)
(1, 'Lô E2a-7, Đường D1, Khu Công nghệ cao', 'TP.HCM', 'Thành phố Thủ Đức', 'Phường Long Thạnh Mỹ', '700000', '720300', '12653', 10.8411, 106.8099, NOW(), NOW()),
(2, 'Số 789 Đường Công Nghiệp', 'Hà Nội', 'Quận Hai Bà Trưng', 'Phường Bách Khoa', '100000', '100300', '20', 21.0056, 105.8433, NOW(), NOW()),
(3, '65 Lê Văn Lương', 'Hồ Chí Minh', 'Quận 7', 'Phường Tân Phong', '700000', '700700', '9233', 10.7329, 106.7065, NOW(), NOW()),
(6, '18 Phan Đình Phùng', 'Hà Nội', 'Quận Ba Đình', 'Phường Quán Thánh', '100000', '100100', '63', 21.0409, 105.8390, NOW(), NOW()),
(7, '720A Điện Biên Phủ', 'Hồ Chí Minh', 'Quận Bình Thạnh', 'Phường 22', '700000', '701600', '8989', 10.7960, 106.7220, NOW(), NOW()),
(8, '72A Nguyễn Trãi', 'Hà Nội', 'Quận Thanh Xuân', 'Phường Thượng Đình', '100000', '100700', '78', 21.0008, 105.8157, NOW(), NOW()),

-- FARM ADDRESSES (IDs 9-31) - Keep existing
(9, 'Xã Tân Thành, H. Châu Thành', 'Đồng Tháp', 'Huyện Châu Thành', 'Xã Tân Thành', '870000', '871100', '12221', 10.2633, 105.7567, NOW(), NOW()),
(10, 'Xã Phước Thuận, H. Xuyên Mộc', 'Bà Rịa - Vũng Tàu', 'Huyện Xuyên Mộc', 'Xã Phước Thuận', '790000', '790300', '9313', 10.5133, 107.4600, NOW(), NOW()),
(11, 'Xã Hàm Thắng, H. Hàm Thuận Bắc', 'Bình Thuận', 'Huyện Hàm Thuận Bắc', 'Xã Hàm Thắng', '790000', '790300', '9318', 10.9833, 108.1167, NOW(), NOW()),
(12, 'Số 12, KDC 16, Ấp 4, Xã Phú Hòa, Định Quán, Đồng Nai', 'Đồng Nai', 'TP. Biên Hòa', 'Xã Phú Hòa', '790000', '790300', '9325', 11.21036900, 107.40797600, NOW(), NOW()),
(13, 'Xã Ea Nuôl, H. Buôn Đôn', 'Đắk Lắk', 'Huyện Buôn Đôn', 'Xã Ea Nuôl', '670000', '670100', '8818', 12.7833, 107.8667, NOW(), NOW()),
(14, 'Xã Lộc Bảo, TP. Bảo Lộc', 'Lâm Đồng', 'TP. Bảo Lộc', 'Xã Lộc Bảo', '670000', '670100', '8819', 11.5067, 107.7633, NOW(), NOW()),
(15, 'Xã Đại Lào, TP. Bảo Lộc', 'Lâm Đồng', 'TP. Bảo Lộc', 'Xã Đại Lào', '670000', '670100', '8820', 11.4800, 107.7500, NOW(), NOW()),
(16, 'Phường 4, TP. Đà Lạt', 'Lâm Đồng', 'TP. Đà Lạt', 'Phường 4', '670000', '670100', '8808', 11.9310, 108.4300, NOW(), NOW()),
(17, 'Phường 5, TP. Đà Lạt', 'Lâm Đồng', 'TP. Đà Lạt', 'Phường 5', '670000', '670100', '8809', 11.9450, 108.4250, NOW(), NOW()),
(18, 'Phường 10, TP. Đà Lạt', 'Lâm Đồng', 'TP. Đà Lạt', 'Phường 10', '670000', '670100', '8814', 11.9350, 108.4600, NOW(), NOW()),
(19, 'xã Ea Tiêu, huyện Cú Kuin, Đắk Lắk', 'Đắk Lắk', 'Huyện Cú Kuin', 'xã Ea Tiêu', '670000', '670100', '8821', 12.62207400, 108.11036000, NOW(), NOW()),
(20, 'Xã Đạ Tẻh, H. Đạ Tẻh', 'Lâm Đồng', 'Huyện Đạ Tẻh', 'Xã Đạ Tẻh', '670000', '670100', '8815', 11.5500, 107.5500, NOW(), NOW()),
(21, 'số 41 Lạc Long Quân, thị trấn Lộc Thắng (huyện Bảo Lâm, tỉnh Lâm Đồng)', 'Lâm Đồng', 'Huyện Bảo Lâm', 'Thị trấn Lộc Thắng', '670000', '670100', '8816', 11.59958400, 107.83705000, NOW(), NOW()),
(22, 'Xã Hưng Tây, H. Hưng Nguyên', 'Nghệ An', 'Huyện Hưng Nguyên', 'Xã Hưng Tây', '460000', '461800', '6401', 18.61578300, 105.67749800, NOW(), NOW()),
(23, 'Xã Hưng Lợi, H. Hưng Nguyên', 'Nghệ An', 'Huyện Hưng Nguyên', 'Xã Hưng Lợi', '460000', '461800', '6408', 18.61995200, 105.67979400, NOW(), NOW()),
(24, 'Xã Hưng Đạo, H. Hưng Nguyên', 'Nghệ An', 'Huyện Hưng Nguyên', 'Xã Hưng Đạo', '460000', '461800', '6402', 18.62264600, 105.67628600, NOW(), NOW()),
(25, 'Xã Tân Yên, H. Lục Nam', 'Bắc Giang', 'Huyện Lục Nam', 'Xã Tân Yên', '460000', '461800', '6418', 21.3000, 106.3000, NOW(), NOW()),
(26, 'Xã Vũ Lăng, H. Vũ Thư', 'Thái Bình', 'Huyện Vũ Thư', 'Xã Vũ Lăng', '460000', '461800', '6419', 20.4167, 106.3167, NOW(), NOW()),
(27, 'Xã Tân Lập, H. Mộc Châu', 'Sơn La', 'Huyện Mộc Châu', 'Xã Tân Lập', '670000', '670100', '8807', 20.8500, 104.6333, NOW(), NOW()),
(28, 'Xã Phú Lương, H. Lương Sơn', 'Hòa Bình', 'Huyện Lương Sơn', 'Xã Phú Lương', '460000', '461800', '6405', 20.8667, 105.5500, NOW(), NOW()),
(29, 'Thôn Trường Thọ, Xã Xuân Trường, Thành phố Đà Lạt, Lâm Đồng', 'Lâm Đồng', 'Thôn Trường Thọ', 'Thành phố Đà Lạt', '460000', '461800', '6411', 11.88986800, 108.56761600, NOW(), NOW()),
(30, 'Xã Phúc Trìu, TP. Phổ Yên', 'Thái Nguyên', 'TP. Phổ Yên', 'Xã Phúc Trìu', '460000', '461800', '6403', 21.4833, 105.7667, NOW(), NOW()),
(31, 'Xã Tà Hộc, H. Bắc Yên', 'Sơn La', 'Huyện Bắc Yên', 'Xã Tà Hộc', '670000', '670100', '8810', 21.0833, 104.3833, NOW(), NOW());

-- =====================================================
-- 2. INSERT USERS (6 Farmers + 20 Vendors)
-- =====================================================
INSERT INTO `users` (`id`, `email`, `password_hash`, `role`, `full_name`, `phone_number`, `tax_code`, `is_verified`, `verification_token`, `verification_sent_at`, `avatar_url`, `status`, `last_login_at`, `created_at`, `updated_at`, `deleted_at`) VALUES
-- System users (Keep IDs 1-16)
(1, 'admin@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'admin', 'Quản trị viên hệ thống', '0901234567', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(2, 'staff1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'staff', 'Nguyễn Văn Nhân Viên 1', '0901234568', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(3, 'staff2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'staff', 'Trần Thị Nhân Viên 2', '0901234569', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(4, 'staff3@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'staff', 'Lê Văn Nhân Viên 3', '0901234570', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(7, 'customer1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Phạm Văn Khách Hàng 1', '0901234573', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(8, 'customer2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Hoàng Thị Khách Hàng 2', '0901234574', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
-- FARMERS (Total 6)
(9, 'farmer1@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Nguyễn Văn Nông Dân 1', '0901234575', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(10, 'farmer2@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Trần Thị Nông Dân 2', '0901234576', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(11, 'testuser@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Người Dùng Test', '0901234577', NULL, 0, 'test-token-123', NOW(), NULL, 'active', NULL, NOW(), NOW(), NULL),
(12, 'inactive@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Người Dùng Không Hoạt Động', '0901234578', NULL, 1, NULL, NULL, NULL, 'inactive', NOW(), NOW(), NOW(), NULL),
(13, 'farmer3@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Lê Văn Nông Dân 3', '0901234580', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(14, 'farmer4@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Phạm Thị Nông Dân 4', '0901234581', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(15, 'farmer5@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Hoàng Văn Nông Dân 5', '0901234582', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(16, 'farmer6@gmail.com', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'customer', 'Vũ Thị Nông Dân 6', '0901234583', NULL, 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
-- VENDORS (20 vendors - IDs 17-36)
(17, 'vendor01@greenfarm.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Phân Bón Xanh Việt', '0902000001', 'MST1000000001', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(18, 'vendor02@biosol.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Vi Sinh Việt', '0902000002', 'MST1000000002', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(19, 'vendor03@ecoagri.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Nông Nghiệp Sinh Thái', '0902000003', 'MST1000000003', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(20, 'vendor04@organicvn.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Hữu Cơ Việt Nam', '0902000004', 'MST1000000004', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(21, 'vendor05@biopest.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Bảo Vệ Thực Vật Sinh Học', '0902000005', 'MST1000000005', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(22, 'vendor06@cleanseeds.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Giống Cây Sạch', '0902000006', 'MST1000000006', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(23, 'vendor07@irrigation.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Hệ Thống Tưới Tiêu', '0902000007', 'MST1000000007', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(24, 'vendor08@smartfarm.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Công Nghệ Nông Nghiệp Thông Minh', '0902000008', 'MST1000000008', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(25, 'vendor09@machinery.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Máy Móc Nông Nghiệp Xanh', '0902000009', 'MST1000000009', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(26, 'vendor10@safeagri.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty An Toàn Nông Nghiệp', '0902000010', 'MST1000000010', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(27, 'vendor11@covermaterial.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Vật Liệu Che Phủ', '0902000011', 'MST1000000011', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(28, 'vendor12@biopack.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Bao Bì Sinh Học', '0902000012', 'MST1000000012', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(29, 'vendor13@hydroponic.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Thủy Canh Việt', '0902000013', 'MST1000000013', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(30, 'vendor14@agritech.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Thiết Bị Nông Nghiệp Công Nghệ Cao', '0902000014', 'MST1000000014', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(31, 'vendor15@greenharvest.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Thu Hoạch Xanh', '0902000015', 'MST1000000015', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(32, 'vendor16@soilcare.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Chăm Sóc Đất', '0902000016', 'MST1000000016', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(33, 'vendor17@plantgrowth.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Kích Thích Sinh Trưởng', '0902000017', 'MST1000000017', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(34, 'vendor18@seedsupply.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Cung Ứng Giống', '0902000018', 'MST1000000018', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(35, 'vendor19@watertech.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Công Nghệ Nước', '0902000019', 'MST1000000019', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL),
(36, 'vendor20@agrisupport.vn', '$2a$11$eebvzn7Au.D1ILICdBn4zeE8kMjPcMwg2CkbCUOiVsWFURxS6JriS', 'vendor', 'Công Ty Hỗ Trợ Nông Nghiệp', '0902000020', 'MST1000000020', 1, NULL, NULL, NULL, 'active', NOW(), NOW(), NOW(), NULL);

-- =====================================================
-- 3. INSERT USER_ADDRESSES
-- =====================================================
INSERT INTO `user_addresses` (`id`, `user_id`, `address_id`, `is_deleted`, `created_at`, `updated_at`, `deleted_at`) VALUES
(1, 1, 1, 0, NOW(), NOW(), NULL),
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
(16, 16, 29, 0, NOW(), NOW(), NULL),
-- Link Vendors to addresses (random from 1,2,3,6,7,8)
(17, 17, 1, 0, NOW(), NOW(), NULL),
(18, 18, 2, 0, NOW(), NOW(), NULL),
(19, 19, 3, 0, NOW(), NOW(), NULL),
(20, 20, 6, 0, NOW(), NOW(), NULL),
(21, 21, 7, 0, NOW(), NOW(), NULL),
(22, 22, 8, 0, NOW(), NOW(), NULL),
(23, 23, 1, 0, NOW(), NOW(), NULL),
(24, 24, 2, 0, NOW(), NOW(), NULL),
(25, 25, 3, 0, NOW(), NOW(), NULL),
(26, 26, 6, 0, NOW(), NOW(), NULL),
(27, 27, 7, 0, NOW(), NOW(), NULL),
(28, 28, 8, 0, NOW(), NOW(), NULL),
(29, 29, 1, 0, NOW(), NOW(), NULL),
(30, 30, 2, 0, NOW(), NOW(), NULL),
(31, 31, 3, 0, NOW(), NOW(), NULL),
(32, 32, 6, 0, NOW(), NOW(), NULL),
(33, 33, 7, 0, NOW(), NOW(), NULL),
(34, 34, 8, 0, NOW(), NOW(), NULL),
(35, 35, 1, 0, NOW(), NOW(), NULL),
(36, 36, 2, 0, NOW(), NOW(), NULL);

-- =====================================================
-- 4. FARM PROFILES & CROPS (Keep existing 23 Farms / 6 Farmers)
-- =====================================================

-- ===== FARMER 1 (User ID 9) - 4 Farms =====
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (1, 9, 'Trang trại Lúa ĐBSCL', 15.5, 9, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(1, 'Lúa', '2025-01-15', 1), (1, 'Rau muống', '2025-06-01', 1), (1, 'Cải thảo', '2025-06-01', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (2, 9, 'Trang trại Cao Su BRVT', 20.0, 10, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(2, 'Cao su', '2020-01-01', 1), (2, 'Điều', '2021-03-01', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (3, 9, 'Trang trại Thanh Long', 8.2, 11, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(3, 'Thanh long ruột đỏ', '2022-05-15', 1), (3, 'Thanh long trắng', '2022-05-15', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (4, 9, 'Trang trại Ca Cao Biên Hòa', 12.0, 12, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(4, 'Ca cao', '2021-08-10', 1);

-- ===== FARMER 2 (User ID 10) - 4 Farms =====
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (5, 10, 'Trang trại Sầu Riêng Đak Lak', 10.5, 13, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(5, 'Sầu riêng Monthong', '2019-06-20', 1), (5, 'Sầu riêng Ri6', '2019-06-20', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (6, 10, 'Trang trại Cà Phê Lâm Đồng', 6.8, 14, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(6, 'Cà phê Arabica', '2018-11-01', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (7, 10, 'Trang trại Chè Lâm Đồng', 5.5, 15, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(7, 'Chè xanh', '2020-02-15', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (8, 10, 'Trang trại Nho Đà Lạt', 3.2, 16, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(8, 'Nho Cardinal', '2023-01-10', 1), (8, 'Nho Kyoho', '2023-01-10', 1);

-- ===== FARMER 3 (User ID 13) - 4 Farms =====
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (9, 13, 'Trang trại Dâu Tây Đà Lạt', 2.5, 17, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(9, 'Dâu tây Camarosa', '2024-10-01', 1), (9, 'Dâu tây Festival', '2024-10-01', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (10, 13, 'Trang trại Rau Sạch Đà Lạt', 4.5, 18, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(10, 'Cà chua bi', '2025-02-01', 1), (10, 'Xà lách', '2025-02-01', 1), (10, 'Bông cải xanh', '2025-02-01', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (11, 13, 'Trang trại Tiêu Lâm Đồng', 7.3, 19, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(11, 'Tiêu (Hồ tiêu)', '2021-05-20', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (12, 13, 'Trang trại Chuối Lâm Đồng', 4.0, 20, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(12, 'Chuối tiêu', '2023-07-15', 1);

-- ===== FARMER 4 (User ID 14) - 4 Farms =====
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (13, 14, 'Trang trại Chè Tây Nguyên', 9.0, 21, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(13, 'Chè shan tuyết', '2019-04-01', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (14, 14, 'Trang trại Lúa Vinh', 18.0, 22, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(14, 'Lúa Nàng Nhen', '2025-01-20', 1), (14, 'Lúa Bắc Hương', '2025-01-20', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (15, 14, 'Trang trại Ngô Vinh', 12.5, 23, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(15, 'Ngô nếp', '2025-03-01', 1), (15, 'Ngô lai', '2025-03-01', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (16, 14, 'Trang trại Sắn Vinh', 10.0, 24, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(16, 'Sắn KM94', '2024-12-15', 1);

-- ===== FARMER 5 (User ID 15) - 4 Farms =====
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (17, 15, 'Trang trại Vải Bắc Giang', 7.5, 25, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(17, 'Vải thiều', '2018-02-10', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (18, 15, 'Trang trại Lúa Thái Bình', 22.0, 26, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(18, 'Lúa Bắc Hương', '2025-01-25', 1), (18, 'Lúa Khang Dân', '2025-01-25', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (19, 15, 'Trang trại Đào Sơn La', 5.8, 27, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(19, 'Đào tiên', '2020-11-05', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (20, 15, 'Trang trại Mận Hòa Bình', 6.2, 28, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(20, 'Mận hậu', '2021-01-15', 1);

-- ===== FARMER 6 (User ID 16) - 3 Farms =====
INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (21, 16, 'Trang trại Hồng Lạng Sơn', 4.5, 29, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(21, 'Hồng giòn', '2022-03-10', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (22, 16, 'Trang trại Chè Thái Nguyên', 8.5, 30, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(22, 'Chè Thái Nguyên', '2020-08-20', 1);

INSERT INTO `farm_profiles` (`id`, `user_id`, `farm_name`, `farm_size_hectares`, `address_id`, `status`, `created_at`, `updated_at`) 
VALUES (23, 16, 'Trang trại Cà Phê Sơn La', 9.5, 31, 'Active', NOW(), NOW());
INSERT INTO `crops` (`farm_profile_id`, `crop_name`, `planting_date`, `is_active`) VALUES 
(23, 'Cà phê Robusta', '2019-09-05', 1);


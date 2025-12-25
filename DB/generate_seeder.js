// -*- coding: utf-8 -*-
/**
 * Script to generate complete SEEDER SQL for VerdantTech
 * Generates: 20 Vendors, 37 Categories, 172 Products, Certificates, Media, Inventory, Orders
 * Node.js version: 14+
 * Updated: 2025-12-11
 */

const fs = require('fs');
const path = require('path');

// =====================================================
// CONSTANTS
// =====================================================

const MAX_WEIGHT_KG = 15.0; // Maximum weight limit
const STOCK_QUANTITY = 10; // Stock quantity for all products
const BATCH_QUANTITY = 10; // Batch inventory quantity
const SERIALS_PER_PRODUCT = 10; // Number of serials per product (for serial-required categories)

// Customer IDs that can place orders (verified and active customers)
const CUSTOMER_IDS = [7, 8, 9, 10, 13, 14, 15, 16];

// Address IDs that customers can use for shipping
const CUSTOMER_ADDRESS_MAP = {
    7: 6,   // customer1 -> address 6
    8: 7,   // customer2 -> address 7
    9: 9,   // farmer1 -> address 9
    10: 13, // farmer2 -> address 13
    13: 17, // farmer3 -> address 17
    14: 22, // farmer4 -> address 22
    15: 25, // farmer5 -> address 25
    16: 29  // farmer6 -> address 29
};

// Date range for orders: Nov 1, 2025 to Dec 11, 2025
const ORDER_START_DATE = new Date('2025-11-01');
const ORDER_END_DATE = new Date('2025-12-11');

// =====================================================
// HELPER FUNCTIONS
// =====================================================

function slugify(text) {
    if (!text) return '';

    // Replace đ/Đ first
    text = text.replace(/đ/g, 'd').replace(/Đ/g, 'D');

    // Normalize to NFD (decompose) to separate base characters from combining marks
    let s = text.normalize('NFD');

    // Remove combining marks (accents)
    s = s.replace(/[\u0300-\u036f]/g, '');

    // Convert to lowercase
    s = s.toLowerCase();

    // Replace all non-alphanumeric characters with dash
    s = s.replace(/[^a-z0-9]+/g, '-');

    // Trim leading/trailing dashes
    s = s.replace(/^-+|-+$/g, '');

    // Limit to 255 characters, cut at last dash if needed
    if (s.length > 255) {
        const cut = s.lastIndexOf('-', 255);
        s = cut > 0 ? s.substring(0, cut) : s.substring(0, 255);
        s = s.replace(/-+$/, '');
    }

    return s;
}

function escapeSQL(str) {
    if (!str) return '';
    return str.replace(/'/g, "''");
}

function formatDateTime(date) {
    return date.toISOString().slice(0, 19).replace('T', ' ');
}

function getRandomDate(start, end) {
    return new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime()));
}

function getRandomElement(arr) {
    return arr[Math.floor(Math.random() * arr.length)];
}

function generateSpecs(productName, categoryName) {
    const specs = {};

    // Phân bón
    if (productName.includes('Phân') || productName.includes('phân')) {
        if (productName.includes('NPK')) {
            const match = productName.match(/NPK\s*([\d-]+)/);
            if (match) specs.npk = match[1];
        }
        if (productName.includes('kg')) {
            const match = productName.match(/(\d+)kg/);
            if (match) specs.weight = match[1] + 'kg';
        }
        specs.type = productName.includes('hữu cơ') ? 'Phân hữu cơ' : 'Phân vi sinh';
    }
    // Thuốc BVTV
    else if (categoryName.includes('Thuốc') || productName.includes('sâu') || productName.includes('bệnh')) {
        if (productName.includes('ppm')) {
            const match = productName.match(/(\d+-?\d*ppm)/);
            if (match) specs.concentration = match[1];
        }
        if (productName.includes('CFU/ml') || productName.includes('bào tử')) {
            specs.concentration = '10^8 CFU/ml';
        }
        specs.form = productName.includes('WP') ? 'WP' : 'EC';
    }
    // Hạt giống
    else if (productName.includes('Hạt giống')) {
        specs.germination_rate = '90-95%';
        specs.pack_size = productName.includes('rau') ? '100g' : '50g';
        specs.type = productName.includes('F1') ? 'F1 Hybrid' : 'Open Pollinated';
    }
    // Máy móc
    else if (productName.includes('Máy') || productName.includes('máy')) {
        const hpMatch = productName.match(/([\d.]+HP)/);
        if (hpMatch) specs.power = hpMatch[1];
        const wMatch = productName.match(/(\d+W)/);
        if (wMatch) specs.power = wMatch[1];
        const litMatch = productName.match(/(\d+)\s*lít/);
        if (litMatch) specs.capacity = litMatch[1] + 'L';
        specs.engine = productName.includes('Honda') ? 'Honda GX200' : 'Electric';
    }
    // Thiết bị điện
    else if (productName.includes('Cảm biến') || productName.includes('Relay') || productName.includes('Bộ') || productName.includes('Van')) {
        const vMatch = productName.match(/(\d+V)/);
        if (vMatch) specs.voltage = vMatch[1];
        if (productName.includes('pH')) specs.range = '0-14 pH';
        if (productName.includes('EC')) specs.range = '0-20 mS/cm';
        specs.interface = 'Analog 4-20mA';
    }
    // Ống/vật liệu
    else if (productName.includes('Ống') || productName.includes('ống')) {
        const mmMatch = productName.match(/(\d+mm)/);
        if (mmMatch) specs.diameter = mmMatch[1];
        if (productName.includes('100m')) specs.length = '100m';
        else if (productName.includes('50m')) specs.length = '50m';
        specs.material = productName.includes('PE') ? 'PE' : 'PVC';
    }

    if (Object.keys(specs).length === 0) {
        specs.type = 'Standard';
        specs.quality = 'Premium';
    }

    return JSON.stringify(specs).replace(/"/g, '\\"');
}

function generateWeightDimensions(productName) {
    let weight = 1.0;
    let dimensions = { length: 20, width: 15, height: 10 };

    // Phân bón (nặng)
    if (productName.includes('phân') || productName.includes('Phân')) {
        if (productName.includes('50kg')) {
            weight = 15.0; // Capped at 15kg
            dimensions = { length: 80, width: 50, height: 20 };
        } else if (productName.includes('20kg')) {
            weight = 15.0; // Capped at 15kg
            dimensions = { length: 60, width: 40, height: 15 };
        } else if (productName.includes('10kg')) {
            weight = 10.0;
            dimensions = { length: 50, width: 30, height: 15 };
        } else if (productName.includes('5kg')) {
            weight = 5.0;
            dimensions = { length: 40, width: 25, height: 10 };
        } else {
            weight = 15.0; // Default for fertilizer capped at 15kg
            dimensions = { length: 60, width: 40, height: 15 };
        }
    }
    // Thuốc BVTV (nhẹ)
    else if (productName.includes('thuốc') || productName.includes('dầu')) {
        weight = 0.5;
        dimensions = { length: 15, width: 10, height: 8 };
    }
    // Hạt giống (rất nhẹ)
    else if (productName.includes('Hạt giống')) {
        weight = 0.1;
        dimensions = { length: 12, width: 8, height: 2 };
    }
    // Máy móc (capped at 15kg)
    else if (productName.includes('Máy')) {
        if (productName.includes('xới đất') || productName.includes('gặt')) {
            weight = 15.0; // Capped at 15kg (was 150)
            dimensions = { length: 180, width: 120, height: 100 };
        } else if (productName.includes('bơm')) {
            weight = 15.0; // Capped at 15kg (was 35)
            dimensions = { length: 60, width: 40, height: 50 };
        } else {
            weight = 15.0;
            dimensions = { length: 50, width: 35, height: 40 };
        }
    }
    // Thiết bị điện (trung bình)
    else if (['Cảm biến', 'Relay', 'Bộ', 'Van'].some(k => productName.includes(k))) {
        weight = 0.8;
        dimensions = { length: 25, width: 20, height: 15 };
    }
    // Ống/vật liệu cuộn (nhẹ nhưng lớn)
    else if (productName.includes('ống') || productName.includes('lưới') || productName.includes('Ống') || productName.includes('Lưới')) {
        if (productName.includes('100m')) {
            weight = 12.0;
            dimensions = { length: 100, width: 30, height: 30 };
        } else {
            weight = 3.0;
            dimensions = { length: 50, width: 20, height: 20 };
        }
    }
    // Dụng cụ cầm tay
    else if (['Cuốc', 'Xẻng', 'Kéo', 'Dao', 'Cào'].some(k => productName.includes(k))) {
        weight = 0.8;
        dimensions = { length: 35, width: 10, height: 5 };
    }
    // Vật liệu nhẹ
    else if (['Túi', 'Hộp', 'Băng', 'Găng tay'].some(k => productName.includes(k))) {
        weight = 0.3;
        dimensions = { length: 30, width: 20, height: 5 };
    }

    // Ensure weight doesn't exceed MAX_WEIGHT_KG
    if (weight > MAX_WEIGHT_KG) {
        weight = MAX_WEIGHT_KG;
    }

    return {
        weight: weight.toFixed(3),
        dimensions: JSON.stringify(dimensions).replace(/"/g, '\\"')
    };
}

function getWarrantyMonths(productName) {
    const machineryKeywords = ['Máy', 'máy', 'Bơm', 'Relay', 'Van điện', 'Cảm biến', 'Bộ điều khiển'];
    const equipmentKeywords = ['Đèn', 'Bẫy', 'Đồng hồ', 'Cân', 'Nhiệt kế'];

    if (machineryKeywords.some(k => productName.includes(k))) return 24;
    if (equipmentKeywords.some(k => productName.includes(k))) return 12;
    return 0;
}

// Categories that require serial numbers (by name):
const SERIAL_REQUIRED_CATEGORY_NAMES = [
    'Hệ Thống Điều Khiển Tưới',
    'Thiết Bị Giám Sát',
    'Hệ Thống IoT & Tự Động Hóa',
    'Máy Móc Nhỏ'
];

function requiresSerial(categoryName) {
    return SERIAL_REQUIRED_CATEGORY_NAMES.includes(categoryName);
}

function getEnergyEfficiencyRating(productName) {
    // Random rating from 1 to 5 based on product name hash for consistency
    const hash = productName.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
    return (hash % 5) + 1; // Returns 1, 2, 3, 4, or 5
}

// =====================================================
// PARSE CSV
// =====================================================

function parseCSV() {
    console.log('📖 Reading CSV file...');
    const csvContent = fs.readFileSync('Products_v2.csv', 'utf8');
    const lines = csvContent.split('\n');
    const products = [];

    // Helper function to parse CSV line properly (handles quotes)
    function parseCSVLine(line) {
        const result = [];
        let current = '';
        let inQuotes = false;

        for (let i = 0; i < line.length; i++) {
            const char = line[i];

            if (char === '"') {
                inQuotes = !inQuotes;
            } else if (char === ',' && !inQuotes) {
                result.push(current);
                current = '';
            } else {
                current += char;
            }
        }
        result.push(current); // Add last field
        return result;
    }

    for (let i = 1; i < lines.length; i++) {
        const line = lines[i].trim();
        if (!line || line.split(',')[0] === '') continue;

        const parts = parseCSVLine(line);
        if (parts.length < 5) continue;

        products.push({
            categoryId: parts[0],
            categoryName: parts[1],
            subId: parts[2],
            subName: parts[3],
            productName: parts[4],
            imageUrl1: parts[6] || '',
            imageUrl2: parts[7] || ''
        });
    }

    console.log(`✅ Parsed ${products.length} products`);
    return products;
}

// =====================================================
// GENERATE SQL PARTS
// =====================================================

function generateVendorData() {
    console.log('🏢 Generating vendor profiles, bank accounts, and wallets...');

    let sql = `-- =====================================================\n`;
    sql += `-- 5. VENDOR PROFILES, BANK ACCOUNTS, WALLETS\n`;
    sql += `-- =====================================================\n\n`;

    // Vendor Profiles
    sql += `-- Insert Vendor Profiles (20 vendors)\n`;
    sql += `INSERT INTO vendor_profiles (id, user_id, company_name, slug, business_registration_number, notes, subscription_active, verified_at, verified_by, created_at, updated_at) VALUES\n`;

    const vendors = [
        'Công Ty Phân Bón Xanh Việt',
        'Công Ty Vi Sinh Việt',
        'Công Ty Nông Nghiệp Sinh Thái',
        'Công Ty Hữu Cơ Việt Nam',
        'Công Ty Bảo Vệ Thực Vật Sinh Học',
        'Công Ty Giống Cây Sạch',
        'Công Ty Hệ Thống Tưới Tiêu',
        'Công Ty Công Nghệ Nông Nghiệp Thông Minh',
        'Công Ty Máy Móc Nông Nghiệp Xanh',
        'Công Ty An Toàn Nông Nghiệp',
        'Công Ty Vật Liệu Che Phủ',
        'Công Ty Bao Bì Sinh Học',
        'Công Ty Thủy Canh Việt',
        'Công Ty Thiết Bị Nông Nghiệp Công Nghệ Cao',
        'Công Ty Thu Hoạch Xanh',
        'Công Ty Chăm Sóc Đất',
        'Công Ty Kích Thích Sinh Trưởng',
        'Công Ty Cung Ứng Giống',
        'Công Ty Công Nghệ Nước',
        'Công Ty Hỗ Trợ Nông Nghiệp'
    ];

    const vendorRows = [];
    vendors.forEach((name, i) => {
        const id = i + 1;
        const userId = 16 + id;
        const slug = slugify(name);
        const brn = `BRN${(1000000000 + id).toString()}`;
        vendorRows.push(`(${id}, ${userId}, '${name}', '${slug}', '${brn}', NULL, 1, NOW(), 1, NOW(), NOW())`);
    });
    sql += vendorRows.join(',\n') + ';\n\n';

    // Wallets
    sql += `-- Insert Wallets (20 vendors - starting balance 10,000,000 VND)\n`;
    sql += `INSERT INTO wallets (id, vendor_id, balance, last_updated_by, created_at, updated_at) VALUES\n`;

    const walletRows = [];
    for (let i = 1; i <= 20; i++) {
        walletRows.push(`(${i}, ${16 + i}, 10000000.00, 1, NOW(), NOW())`);
    }
    sql += walletRows.join(',\n') + ';\n\n';

    console.log('✅ Generated 20 vendor profiles, 20 wallets');
    return sql;
}

function generateVendorCertificates() {
    const certificates = [
        { code: 'TCVN_7259', name: 'TCVN 7259:2003 - Phân hữu cơ' },
        { code: 'TCVN_8956', name: 'TCVN 8956:2011 - Phân vi sinh' },
        { code: 'ISO_14001', name: 'ISO 14001 Environmental Management' },
        { code: 'VIETGAP', name: 'VietGAP – Thực hành nông nghiệp tốt tại Việt Nam' },
        { code: 'USDA_ORGANIC', name: 'USDA Organic Certification' }
    ];

    let sql = `-- Insert Vendor Certificates (20 vendors x 1 cert each = 20 certs)\n`;
    sql += `INSERT INTO vendor_certificates (id, vendor_id, certification_code, certification_name, status, rejection_reason, uploaded_at, verified_at, verified_by, created_at, updated_at) VALUES\n`;

    const rows = [];
    for (let i = 1; i <= 20; i++) {
        const cert = certificates[i % certificates.length];
        rows.push(`(${i}, ${16 + i}, '${cert.code}', '${cert.name}', 'verified', NULL, NOW(), NOW(), 1, NOW(), NOW())`);
    }
    sql += rows.join(',\n') + ';\n\n';

    // Media links for vendor certificates
    sql += `-- Media Links for Vendor Certificates (20 PDF files)\n`;
    sql += `INSERT INTO media_links (id, owner_type, owner_id, image_url, image_public_id, purpose, sort_order, created_at, updated_at) VALUES\n`;

    const mediaRows = [];
    for (let i = 1; i <= 20; i++) {
        mediaRows.push(`(${i}, 'vendor_certificates', ${i}, 'https://res.cloudinary.com/verdanttech/certificates/vendor_cert_${i}.pdf', 'vendor_cert_${i}', 'vendorCertificatesPdf', 0, NOW(), NOW())`);
    }
    sql += mediaRows.join(',\n') + ';\n\n';

    console.log('✅ Generated 20 vendor certificates');
    return sql;
}

function generateCategories(products) {
    console.log('🏷️  Generating categories...');

    // Extract unique categories
    const categoryMap = new Map();
    products.forEach(p => {
        const catKey = `${p.categoryId}`;
        if (!categoryMap.has(catKey)) {
            categoryMap.set(catKey, {
                id: p.categoryId,
                name: p.categoryName,
                subs: new Map()
            });
        }

        const cat = categoryMap.get(catKey);
        const subKey = `${p.subId}`;
        // Chỉ tạo sub-category nếu subId KHÁC categoryId
        if (p.subId !== p.categoryId && !cat.subs.has(subKey)) {
            cat.subs.set(subKey, {
                id: p.subId,
                name: p.subName
            });
        }
    });

    const totalSubs = Array.from(categoryMap.values()).reduce((sum, cat) => sum + cat.subs.size, 0);
    const totalParents = categoryMap.size;
    let sql = `-- Insert Product Categories (${totalParents} parent + ${totalSubs} sub categories)\n`;
    sql += `INSERT INTO product_categories (id, parent_id, name, slug, description, is_active, serial_required, created_at, updated_at) VALUES\n`;

    const rows = [];
    let catId = 1;

    // Parent categories
    for (const [key, cat] of categoryMap) {
        const slug = slugify(cat.name);
        rows.push(`(${catId}, NULL, '${escapeSQL(cat.name)}', '${slug}', 'Danh mục ${escapeSQL(cat.name)}', 1, 0, NOW(), NOW())`);
        cat.dbId = catId;
        catId++;
    }

    // Sub categories
    for (const [key, cat] of categoryMap) {
        for (const [subKey, sub] of cat.subs) {
            const slug = slugify(sub.name);
            // Check if this sub-category requires serial numbers
            const serialRequired = requiresSerial(sub.name) ? 1 : 0;
            rows.push(`(${catId}, ${cat.dbId}, '${escapeSQL(sub.name)}', '${slug}', 'Danh mục con ${escapeSQL(sub.name)}', 1, ${serialRequired}, NOW(), NOW())`);
            sub.dbId = catId;
            catId++;
        }
    }

    sql += rows.join(',\n') + ';\n\n';

    console.log(`✅ Generated ${rows.length} categories`);
    return { sql, categoryMap };
}

function generateProducts(products, categoryMap) {
    console.log('📦 Generating products...');

    let sql = `-- Insert Products (${products.length} products, price=5000, commission_rate=10%, stock=${STOCK_QUANTITY})\n`;
    sql += `INSERT INTO products (id, category_id, vendor_id, product_code, product_name, slug, description, unit_price, commission_rate, discount_percentage, energy_efficiency_rating, specifications, manual_urls, public_url, warranty_months, stock_quantity, weight_kg, dimensions_cm, is_active, view_count, sold_count, rating_average, registration_id, created_at, updated_at) VALUES\n`;

    const rows = [];
    let productId = 1;
    let vendorId = 17; // Start from vendor 17 (first vendor)
    let productsPerVendor = Math.ceil(products.length / 20); // ~9 products per vendor

    products.forEach((p, index) => {
        // Find category DB ID
        const cat = categoryMap.get(p.categoryId);
        if (!cat) {
            console.warn(`Warning: Category ${p.categoryId} not found for product ${p.productName}`);
            return;
        }

        // Nếu subId === categoryId → không có sub, dùng parent
        let categoryDbId;
        let categoryName; // Store category name for serial checking
        if (p.subId === p.categoryId) {
            categoryDbId = cat.dbId;  // Gắn vào parent
            categoryName = cat.name;
        } else {
            // Có sub-category → tìm và dùng sub
            const sub = Array.from(cat.subs.values()).find(s => s.id === p.subId);
            if (!sub) {
                console.warn(`Warning: Sub-category ${p.subId} not found for product ${p.productName}`);
                return;
            }
            categoryDbId = sub.dbId;
            categoryName = sub.name;
        }
        const productCode = `PRD-C${p.categoryId.padStart(2, '0')}-${productId.toString().padStart(4, '0')}`;
        const slug = slugify(p.productName);
        const specs = generateSpecs(p.productName, p.categoryName);
        const { weight, dimensions } = generateWeightDimensions(p.productName);
        const warranty = getWarrantyMonths(p.productName);
        const energyRating = getEnergyEfficiencyRating(p.productName);

        // Rotate vendors every N products
        if (index > 0 && index % productsPerVendor === 0 && vendorId < 36) {
            vendorId++;
        }

        rows.push(`(${productId}, ${categoryDbId}, ${vendorId}, '${productCode}', '${escapeSQL(p.productName)}', '${slug}', 'Sản phẩm ${escapeSQL(p.productName)} chất lượng cao', 5000.00, 10.00, 0.00, ${energyRating}, "${specs}", NULL, NULL, ${warranty}, ${STOCK_QUANTITY}, ${weight}, "${dimensions}", 1, 0, 0, 0.00, NULL, NOW(), NOW())`);

        p.dbId = productId;
        p.vendorId = vendorId;
        p.categoryDbId = categoryDbId; // Store category DB ID for serial generation
        p.categoryName = categoryName; // Store category name for serial checking
        p.productId = productId; // Store product ID as well
        p.weight = parseFloat(weight);
        p.unitPrice = 5000.00;
        productId++;
    });

    sql += rows.join(',\n') + ';\n\n';

    console.log(`✅ Generated ${rows.length} products`);
    return sql;
}

function generateProductMediaLinks(products) {
    console.log('🖼️  Generating product media links...');

    let sql = `-- Insert Media Links for Products (${products.filter(p => p.imageUrl1).length * 2} images)\n`;
    sql += `INSERT INTO media_links (id, owner_type, owner_id, image_url, image_public_id, purpose, sort_order, created_at, updated_at) VALUES\n`;

    const rows = [];
    let mediaId = 21; // Start after vendor certificates (1-20)

    products.forEach(p => {
        if (!p.dbId) return;

        // Image 1
        if (p.imageUrl1) {
            const publicId = p.imageUrl1.split('/').pop().split('.')[0];
            rows.push(`(${mediaId}, 'products', ${p.dbId}, '${escapeSQL(p.imageUrl1)}', '${publicId}', 'none', 0, NOW(), NOW())`);
            mediaId++;
        }

        // Image 2
        if (p.imageUrl2) {
            const publicId = p.imageUrl2.split('/').pop().split('.')[0];
            rows.push(`(${mediaId}, 'products', ${p.dbId}, '${escapeSQL(p.imageUrl2)}', '${publicId}', 'none', 1, NOW(), NOW())`);
            mediaId++;
        }
    });

    sql += rows.join(',\n') + ';\n\n';
    console.log(`✅ Generated ${rows.length} media links`);
    return { sql, lastMediaId: mediaId };
}

function generateProductCertificates(products, startMediaId) {
    console.log('📜 Generating product certificates...');

    // Mapping theo agriculture_products.md
    const certRequirements = {
        '1': 'TCVN_7259', // Dinh dưỡng thực vật -> Phân bón
        '2': 'THUOC_BVTV', // Bảo vệ thực vật -> Thuốc BVTV
        '3.1': 'GIONG_CAY', // Hạt giống rau -> Giấy chứng nhận giống
        '4.3': 'CERT_CR', // Thiết bị bơm -> Hợp quy CR
        '4.4': 'CERT_CR', // Hệ thống điều khiển -> Hợp quy CR
        '5': 'CERT_CR', // Công nghệ thông minh -> Hợp quy CR
        '6.1': 'CERT_CR', // Máy móc nhỏ -> Hợp quy CR
        '7.1': 'TCVN_TEST', // Bộ test -> TCVN
        '7.2': 'TCVN_BAO_HO', // Bảo hộ lao động -> TCVN
        '9.1': 'QCVN_BYT' // Bao bì sinh học -> QCVN BYT
    };

    let sql = `-- Insert Product Certificates (only for products requiring mandatory certs)\n`;
    sql += `INSERT INTO product_certificates (id, product_id, registration_id, certification_code, certification_name, status, rejection_reason, uploaded_at, verified_at, verified_by, created_at, updated_at) VALUES\n`;

    const rows = [];
    let certId = 1;
    let mediaId = startMediaId;

    products.forEach(p => {
        if (!p.dbId) return;

        const key = p.subId;
        if (certRequirements[key]) {
            const code = certRequirements[key];
            let name = '';

            switch (code) {
                case 'TCVN_7259':
                    name = 'TCVN 7259:2003 - Giấy chứng nhận lưu hành phân bón';
                    break;
                case 'THUOC_BVTV':
                    name = 'Giấy chứng nhận đăng ký thuốc BVTV - Cục BVTV';
                    break;
                case 'GIONG_CAY':
                    name = 'Giấy chứng nhận giống cây trồng - Cục Trồng trọt';
                    break;
                case 'CERT_CR':
                    name = 'Giấy chứng nhận hợp quy (CR) - An toàn điện';
                    break;
                case 'TCVN_TEST':
                    name = 'TCVN - Độ chính xác thiết bị kiểm tra';
                    break;
                case 'TCVN_BAO_HO':
                    name = 'TCVN - An toàn lao động';
                    break;
                case 'QCVN_BYT':
                    name = 'QCVN 12-1:2011/BYT - An toàn vệ sinh thực phẩm';
                    break;
            }

            rows.push(`(${certId}, ${p.dbId}, NULL, '${code}', '${name}', 'verified', NULL, NOW(), NOW(), 1, NOW(), NOW())`);
            certId++;
        }
    });

    if (rows.length > 0) {
        sql += rows.join(',\n') + ';\n\n';

        // Media links for product certificates
        sql += `-- Media Links for Product Certificates (${rows.length} PDF files)\n`;
        sql += `INSERT INTO media_links (id, owner_type, owner_id, image_url, image_public_id, purpose, sort_order, created_at, updated_at) VALUES\n`;

        const mediaRows = [];
        for (let i = 1; i <= rows.length; i++) {
            mediaRows.push(`(${mediaId}, 'product_certificates', ${i}, 'https://res.cloudinary.com/verdanttech/certificates/product_cert_${i}.pdf', 'product_cert_${i}', 'productcertificatepdf', 0, NOW(), NOW())`);
            mediaId++;
        }
        sql += mediaRows.join(',\n') + ';\n\n';
    }

    console.log(`✅ Generated ${rows.length} product certificates`);
    return sql;
}

function generateBatchInventory(products) {
    console.log('📦 Generating batch inventory...');

    let sql = `-- Insert Batch Inventory (${products.length} batches, quantity=${BATCH_QUANTITY})\n`;
    sql += `INSERT INTO batch_inventory (id, product_id, sku, vendor_id, batch_number, lot_number, quantity, unit_cost_price, expiry_date, manufacturing_date, notes, created_at, updated_at) VALUES\n`;

    const rows = [];
    products.forEach((p, index) => {
        if (!p.dbId) return;

        const batchId = p.dbId;
        const sku = `SKU-${p.dbId.toString().padStart(4, '0')}`;
        const batchNum = `BATCH${(index + 1).toString().padStart(4, '0')}`;
        const lotNum = `LOT${(index + 1).toString().padStart(4, '0')}`;
        const unitCost = '900.00'; // Cost = 900, selling price = 5000

        // Expiry date only for fertilizers, seeds, chemicals
        let expiryDate = 'NULL';
        if (p.categoryId === '1' || p.categoryId === '2' || p.categoryId === '3') {
            expiryDate = "'2026-12-31'";
        }

        // Store lot number for order generation
        p.lotNumber = lotNum;

        rows.push(`(${batchId}, ${p.dbId}, '${sku}', ${p.vendorId}, '${batchNum}', '${lotNum}', ${BATCH_QUANTITY}, ${unitCost}, ${expiryDate}, '2025-01-01', 'Nhập kho đợt đầu', NOW(), NOW())`);
    });

    sql += rows.join(',\n') + ';\n\n';

    // Product serials for categories that require serial numbers
    const serialRequiredProducts = products.filter(p => {
        if (!p.dbId || !p.categoryName) return false;
        return requiresSerial(p.categoryName);
    });

    if (serialRequiredProducts.length > 0) {
        sql += `-- Insert Product Serials for Serial-Required Categories (${serialRequiredProducts.length} products x ${SERIALS_PER_PRODUCT} units = ${serialRequiredProducts.length * SERIALS_PER_PRODUCT} serials)\n`;
        sql += `-- Categories: Máy Móc Nhỏ, Hệ Thống IoT & Tự Động Hóa, Thiết Bị Giám Sát, Hệ Thống Điều Khiển Tưới\n`;
        sql += `INSERT INTO product_serials (id, batch_inventory_id, product_id, serial_number, status, created_at, updated_at) VALUES\n`;

        const serialRows = [];
        let serialId = 1;

        serialRequiredProducts.forEach(p => {
            if (!p.dbId) return;

            // Store serials for order generation
            p.serials = [];

            // Generate serials per product (all status = 'stock')
            for (let i = 1; i <= SERIALS_PER_PRODUCT; i++) {
                const serialNum = `SN-${p.dbId.toString().padStart(4, '0')}-${i.toString().padStart(3, '0')}`;
                p.serials.push({ id: serialId, serialNumber: serialNum });
                serialRows.push(`(${serialId}, ${p.dbId}, ${p.dbId}, '${serialNum}', 'stock', NOW(), NOW())`);
                serialId++;
            }
        });

        sql += serialRows.join(',\n') + ';\n\n';
        console.log(`✅ Generated ${serialRows.length} product serials for ${serialRequiredProducts.length} products`);
    }

    console.log(`✅ Generated ${rows.length} batch inventory records`);
    return sql;
}

function generateOrders(products) {
    console.log('🛒 Generating 60 orders for vendor01 and vendor02 products...');

    // Get products from vendor01 (user_id=17) and vendor02 (user_id=18)
    const vendor01Products = products.filter(p => p.vendorId === 17 && p.dbId);
    const vendor02Products = products.filter(p => p.vendorId === 18 && p.dbId);
    const targetProducts = [...vendor01Products, ...vendor02Products];

    console.log(`   Vendor01 products: ${vendor01Products.length}`);
    console.log(`   Vendor02 products: ${vendor02Products.length}`);
    console.log(`   Total target products: ${targetProducts.length}`);

    if (targetProducts.length === 0) {
        console.warn('Warning: No products found for vendor01 and vendor02');
        return '';
    }

    let sql = `-- =====================================================\n`;
    sql += `-- 6. ORDERS, ORDER DETAILS, TRANSACTIONS, PAYMENTS, EXPORT INVENTORY\n`;
    sql += `-- =====================================================\n\n`;

    const orders = [];
    const orderDetails = [];
    const transactions = [];
    const payments = [];
    const exportInventories = [];

    let orderId = 1;
    let orderDetailId = 1;
    let transactionId = 1;
    let exportId = 1;

    // Track serial usage per product
    const serialUsage = new Map(); // productId -> next available serial index

    // Track remaining stock per product (start with STOCK_QUANTITY)
    const remainingStock = new Map(); // productId -> remaining quantity
    targetProducts.forEach(p => remainingStock.set(p.dbId, STOCK_QUANTITY));

    // Generate 60 orders
    for (let i = 0; i < 60; i++) {
        const customerId = getRandomElement(CUSTOMER_IDS);
        const addressId = CUSTOMER_ADDRESS_MAP[customerId];
        const orderDate = getRandomDate(ORDER_START_DATE, ORDER_END_DATE);

        // Random number of products per order (1-3)
        const numProducts = Math.floor(Math.random() * 3) + 1;
        const selectedProducts = [];
        const usedProductIds = new Set();

        // Get products that still have stock
        const availableProducts = targetProducts.filter(p => remainingStock.get(p.dbId) > 0);

        if (availableProducts.length === 0) {
            console.log(`   Order ${i + 1}: No products available in stock, skipping...`);
            continue;
        }

        for (let j = 0; j < numProducts && j < availableProducts.length; j++) {
            let product;
            let attempts = 0;
            do {
                product = getRandomElement(availableProducts);
                attempts++;
            } while ((usedProductIds.has(product.dbId) || remainingStock.get(product.dbId) <= 0) && attempts < 20);

            if (!usedProductIds.has(product.dbId) && remainingStock.get(product.dbId) > 0) {
                usedProductIds.add(product.dbId);
                selectedProducts.push(product);
                // Decrease remaining stock
                remainingStock.set(product.dbId, remainingStock.get(product.dbId) - 1);
            }
        }

        if (selectedProducts.length === 0) continue;

        // Calculate order totals
        let subtotal = 0;
        const orderDetailItems = [];

        selectedProducts.forEach(product => {
            const quantity = 1; // Each order has quantity 1 per product
            const unitPrice = product.unitPrice || 5000.00;
            const lineSubtotal = unitPrice * quantity;
            subtotal += lineSubtotal;

            orderDetailItems.push({
                orderDetailId: orderDetailId,
                productId: product.dbId,
                quantity: quantity,
                unitPrice: unitPrice,
                subtotal: lineSubtotal,
                product: product
            });
            orderDetailId++;
        });

        const shippingFee = 30000; // Fixed shipping fee
        const totalAmount = subtotal + shippingFee;

        // Calculate dates
        const confirmedAt = new Date(orderDate.getTime() + 1 * 24 * 60 * 60 * 1000); // +1 day
        const shippedAt = new Date(orderDate.getTime() + 2 * 24 * 60 * 60 * 1000); // +2 days
        const deliveredAt = new Date(orderDate.getTime() + 5 * 24 * 60 * 60 * 1000); // +5 days

        // Create order
        orders.push({
            id: orderId,
            customerId: customerId,
            status: 'delivered',
            subtotal: subtotal.toFixed(2),
            taxAmount: '0.00',
            shippingFee: shippingFee.toFixed(2),
            discountAmount: '0.00',
            totalAmount: totalAmount.toFixed(2),
            addressId: addressId,
            orderPaymentMethod: 'banking',
            shippingMethod: 'GHTK Express',
            trackingNumber: `VT${orderId.toString().padStart(10, '0')}`,
            courierId: 1,
            width: 30,
            height: 20,
            length: 40,
            weight: 1000,
            isWalletCredited: 1,
            createdAt: formatDateTime(orderDate),
            confirmedAt: formatDateTime(confirmedAt),
            shippedAt: formatDateTime(shippedAt),
            deliveredAt: formatDateTime(deliveredAt)
        });

        // Create order details
        orderDetailItems.forEach(item => {
            orderDetails.push({
                id: item.orderDetailId,
                orderId: orderId,
                productId: item.productId,
                quantity: item.quantity,
                unitPrice: item.unitPrice.toFixed(2),
                discountAmount: '0.00',
                subtotal: item.subtotal.toFixed(2),
                isRefunded: 0
            });

            // Create export inventory
            const product = item.product;
            const isSerialRequired = product.categoryName && requiresSerial(product.categoryName);

            if (isSerialRequired && product.serials && product.serials.length > 0) {
                // Get next available serial
                let serialIdx = serialUsage.get(product.dbId) || 0;
                if (serialIdx < product.serials.length) {
                    const serial = product.serials[serialIdx];
                    serialUsage.set(product.dbId, serialIdx + 1);

                    exportInventories.push({
                        id: exportId,
                        productId: product.dbId,
                        productSerialId: serial.id,
                        lotNumber: product.lotNumber,
                        orderDetailId: item.orderDetailId,
                        quantity: 1,
                        movementType: 'sale',
                        createdBy: 2 // staff1
                    });
                    exportId++;
                }
            } else {
                // No serial required
                exportInventories.push({
                    id: exportId,
                    productId: product.dbId,
                    productSerialId: 'NULL',
                    lotNumber: product.lotNumber,
                    orderDetailId: item.orderDetailId,
                    quantity: item.quantity,
                    movementType: 'sale',
                    createdBy: 2 // staff1
                });
                exportId++;
            }
        });

        // Create transaction (PayOS - banking)
        const gatewayPaymentId = Date.now() + orderId;
        transactions.push({
            id: transactionId,
            transactionType: 'payment_in',
            amount: totalAmount.toFixed(2),
            currency: 'VND',
            userId: customerId,
            orderId: orderId,
            status: 'completed',
            note: `Thanh toán đơn hàng #${orderId} qua PayOS`,
            gatewayPaymentId: gatewayPaymentId.toString(),
            createdBy: customerId,
            processedBy: 2,
            processedAt: formatDateTime(confirmedAt)
        });

        // Create payment
        payments.push({
            id: transactionId,
            transactionId: transactionId,
            paymentMethod: 'payos',
            paymentGateway: 'payos'
        });

        transactionId++;
        orderId++;
    }

    // Generate SQL for orders
    sql += `-- Insert Orders (60 completed orders for vendor01 & vendor02 products)\n`;
    sql += `INSERT INTO orders (id, customer_id, status, subtotal, tax_amount, shipping_fee, discount_amount, total_amount, address_id, order_payment_method, shipping_method, tracking_number, courier_id, width, height, length, weight, is_wallet_credited, confirmed_at, shipped_at, delivered_at, created_at, updated_at) VALUES\n`;

    const orderRows = orders.map(o =>
        `(${o.id}, ${o.customerId}, '${o.status}', ${o.subtotal}, ${o.taxAmount}, ${o.shippingFee}, ${o.discountAmount}, ${o.totalAmount}, ${o.addressId}, '${o.orderPaymentMethod}', '${o.shippingMethod}', '${o.trackingNumber}', ${o.courierId}, ${o.width}, ${o.height}, ${o.length}, ${o.weight}, ${o.isWalletCredited}, '${o.confirmedAt}', '${o.shippedAt}', '${o.deliveredAt}', '${o.createdAt}', '${o.createdAt}')`
    );
    sql += orderRows.join(',\n') + ';\n\n';

    // Generate SQL for order details
    sql += `-- Insert Order Details\n`;
    sql += `INSERT INTO order_details (id, order_id, product_id, quantity, unit_price, discount_amount, subtotal, is_refunded, updated_at) VALUES\n`;

    const detailRows = orderDetails.map(d =>
        `(${d.id}, ${d.orderId}, ${d.productId}, ${d.quantity}, ${d.unitPrice}, ${d.discountAmount}, ${d.subtotal}, ${d.isRefunded}, NOW())`
    );
    sql += detailRows.join(',\n') + ';\n\n';

    // Generate SQL for transactions
    sql += `-- Insert Transactions (PayOS payments)\n`;
    sql += `INSERT INTO transactions (id, transaction_type, amount, currency, user_id, order_id, status, note, gateway_payment_id, created_by, processed_by, processed_at, created_at, updated_at) VALUES\n`;

    const transRows = transactions.map(t =>
        `(${t.id}, '${t.transactionType}', ${t.amount}, '${t.currency}', ${t.userId}, ${t.orderId}, '${t.status}', '${escapeSQL(t.note)}', '${t.gatewayPaymentId}', ${t.createdBy}, ${t.processedBy}, '${t.processedAt}', '${t.processedAt}', '${t.processedAt}')`
    );
    sql += transRows.join(',\n') + ';\n\n';

    // Generate SQL for payments
    sql += `-- Insert Payments\n`;
    sql += `INSERT INTO payments (id, transaction_id, payment_method, payment_gateway, gateway_response, created_at, updated_at) VALUES\n`;

    const paymentRows = payments.map(p =>
        `(${p.id}, ${p.transactionId}, '${p.paymentMethod}', '${p.paymentGateway}', '{}', NOW(), NOW())`
    );
    sql += paymentRows.join(',\n') + ';\n\n';

    // Generate SQL for export inventory
    sql += `-- Insert Export Inventory (stock out for delivered orders)\n`;
    sql += `INSERT INTO export_inventory (id, product_id, product_serial_id, lot_number, order_detail_id, quantity, refund_quantity, movement_type, created_by, created_at, updated_at) VALUES\n`;

    const exportRows = exportInventories.map(e =>
        `(${e.id}, ${e.productId}, ${e.productSerialId}, '${e.lotNumber}', ${e.orderDetailId}, ${e.quantity}, 0, '${e.movementType}', ${e.createdBy}, NOW(), NOW())`
    );
    sql += exportRows.join(',\n') + ';\n\n';

    // Update product serials that were sold
    const soldSerials = exportInventories.filter(e => e.productSerialId !== 'NULL').map(e => e.productSerialId);
    if (soldSerials.length > 0) {
        sql += `-- Update sold product serials status\n`;
        sql += `UPDATE product_serials SET status = 'sold', updated_at = NOW() WHERE id IN (${soldSerials.join(', ')});\n\n`;
    }

    // Calculate total quantity sold per product and update stock_quantity & sold_count
    const productSoldQuantities = new Map(); // productId -> total quantity sold
    orderDetails.forEach(d => {
        const current = productSoldQuantities.get(d.productId) || 0;
        productSoldQuantities.set(d.productId, current + d.quantity);
    });

    if (productSoldQuantities.size > 0) {
        sql += `-- Update product stock_quantity and sold_count after orders\n`;
        productSoldQuantities.forEach((quantitySold, productId) => {
            const newStock = STOCK_QUANTITY - quantitySold;
            sql += `UPDATE products SET stock_quantity = ${newStock}, sold_count = ${quantitySold}, updated_at = NOW() WHERE id = ${productId};\n`;
        });
        sql += '\n';
    }

    // Also update batch_inventory quantity
    sql += `-- Update batch_inventory quantity after export\n`;
    productSoldQuantities.forEach((quantitySold, productId) => {
        const newBatchQty = BATCH_QUANTITY - quantitySold;
        sql += `UPDATE batch_inventory SET quantity = ${newBatchQty}, updated_at = NOW() WHERE product_id = ${productId};\n`;
    });
    sql += '\n';

    console.log(`✅ Generated ${orders.length} orders, ${orderDetails.length} order details, ${transactions.length} transactions, ${exportInventories.length} export records`);
    console.log(`   Updated stock for ${productSoldQuantities.size} products`);
    return { sql, orderDetails, orders };
}

function generateProductReviews(orderDetails, orders) {
    console.log('⭐ Generating product reviews for purchased products...');

    // Review comments in Vietnamese
    const positiveComments = [
        'Sản phẩm rất tốt, đúng như mô tả. Giao hàng nhanh, đóng gói cẩn thận.',
        'Chất lượng tuyệt vời, mình rất hài lòng. Sẽ mua lại lần sau.',
        'Sản phẩm chính hãng, giá cả hợp lý. Shop tư vấn nhiệt tình.',
        'Hàng đẹp, chất lượng tốt. Đóng gói kỹ càng, giao hàng đúng hẹn.',
        'Rất hài lòng với sản phẩm này. Hiệu quả sử dụng cao.',
        'Sản phẩm đúng như hình ảnh và mô tả. Mình đã dùng thử rất tốt.',
        'Chất lượng vượt mong đợi. Sẽ giới thiệu cho bạn bè.',
        'Giao hàng siêu nhanh, sản phẩm chất lượng cao. 5 sao!',
        'Đã mua nhiều lần, lần nào cũng hài lòng. Shop uy tín.',
        'Sản phẩm tốt, phù hợp với nông nghiệp hữu cơ. Recommend!'
    ];

    const neutralComments = [
        'Sản phẩm ổn, đúng mô tả. Giao hàng hơi lâu một chút.',
        'Chất lượng khá tốt, giá cả phải chăng.',
        'Sản phẩm dùng được, không có gì đặc biệt.',
        'Hàng đúng như mô tả, giao hàng bình thường.',
        'Sản phẩm tạm ổn với mức giá này.'
    ];

    const negativeComments = [
        'Sản phẩm tạm được nhưng đóng gói chưa kỹ lắm.',
        'Giao hàng hơi lâu, sản phẩm bình thường.'
    ];

    // Group order details by product to count purchases
    const productPurchases = new Map(); // productId -> [{orderId, customerId, deliveredAt}]

    orderDetails.forEach(detail => {
        const order = orders.find(o => o.id === detail.orderId);
        if (!order) return;

        if (!productPurchases.has(detail.productId)) {
            productPurchases.set(detail.productId, []);
        }
        productPurchases.get(detail.productId).push({
            orderId: detail.orderId,
            customerId: order.customerId,
            deliveredAt: order.deliveredAt
        });
    });

    const reviews = [];
    let reviewId = 1;

    // Generate reviews for each product
    productPurchases.forEach((purchases, productId) => {
        // Determine number of reviews: 2-3 if bought <=3 times, up to 5 if bought more
        let numReviews;
        if (purchases.length <= 3) {
            numReviews = Math.min(purchases.length, Math.floor(Math.random() * 2) + 2); // 2-3 reviews
        } else {
            numReviews = Math.min(purchases.length, 5); // Up to 5 reviews
        }

        // Select random purchases to review (avoid duplicate customer reviews for same product)
        const usedCustomers = new Set();
        const selectedPurchases = [];

        for (const purchase of purchases) {
            if (!usedCustomers.has(purchase.customerId) && selectedPurchases.length < numReviews) {
                usedCustomers.add(purchase.customerId);
                selectedPurchases.push(purchase);
            }
        }

        // Generate reviews
        selectedPurchases.forEach(purchase => {
            // Rating distribution: mostly 4-5 stars (80%), some 3 stars (15%), rare 2 stars (5%)
            let rating;
            const rand = Math.random();
            if (rand < 0.45) rating = 5;
            else if (rand < 0.80) rating = 4;
            else if (rand < 0.95) rating = 3;
            else rating = 2;

            // Select comment based on rating
            let comment;
            if (rating >= 4) {
                comment = getRandomElement(positiveComments);
            } else if (rating === 3) {
                comment = getRandomElement(neutralComments);
            } else {
                comment = getRandomElement(negativeComments);
            }

            // Review date is after delivery date (1-5 days later)
            const deliveredDate = new Date(purchase.deliveredAt);
            const reviewDate = new Date(deliveredDate.getTime() + (Math.random() * 5 + 1) * 24 * 60 * 60 * 1000);

            reviews.push({
                id: reviewId,
                productId: productId,
                orderId: purchase.orderId,
                customerId: purchase.customerId,
                rating: rating,
                comment: comment,
                createdAt: formatDateTime(reviewDate)
            });
            reviewId++;
        });
    });

    if (reviews.length === 0) {
        console.log('⚠️ No reviews generated');
        return '';
    }

    let sql = `-- =====================================================\n`;
    sql += `-- 7. PRODUCT REVIEWS\n`;
    sql += `-- =====================================================\n\n`;

    sql += `-- Insert Product Reviews (${reviews.length} reviews for products from vendor01 & vendor02)\n`;
    sql += `INSERT INTO product_reviews (id, product_id, order_id, customer_id, rating, comment, created_at, updated_at) VALUES\n`;

    const reviewRows = reviews.map(r =>
        `(${r.id}, ${r.productId}, ${r.orderId}, ${r.customerId}, ${r.rating}, '${escapeSQL(r.comment)}', '${r.createdAt}', '${r.createdAt}')`
    );
    sql += reviewRows.join(',\n') + ';\n\n';

    // Calculate average rating per product for update
    const productRatings = new Map();
    reviews.forEach(r => {
        if (!productRatings.has(r.productId)) {
            productRatings.set(r.productId, { sum: 0, count: 0 });
        }
        const data = productRatings.get(r.productId);
        data.sum += r.rating;
        data.count++;
    });

    // Generate UPDATE statements for product rating_average
    sql += `-- Update product rating averages\n`;
    productRatings.forEach((data, productId) => {
        const avg = (data.sum / data.count).toFixed(2);
        sql += `UPDATE products SET rating_average = ${avg}, updated_at = NOW() WHERE id = ${productId};\n`;
    });
    sql += '\n';

    console.log(`✅ Generated ${reviews.length} product reviews for ${productPurchases.size} products`);
    return sql;
}

function getOldSEEDERParts() {
    console.log('📋 Preserving old SEEDER parts (forum, chatbot, etc.)...');

    return `
-- =====================================================
-- PRESERVE EXISTING SYSTEM DATA (Forum, Chatbot)
-- =====================================================

-- Insert Forum Categories
INSERT INTO forum_categories (id, name, description, is_active, created_at, updated_at) VALUES
(1, 'Kỹ Thuật Canh Tác', 'Thảo luận về các phương pháp canh tác bền vững và hữu cơ', 1, NOW(), NOW()),
(2, 'Máy Móc & Thiết Bị Nông Nghiệp', 'Chia sẻ kinh nghiệm sử dụng máy móc hạng nặng và thiết bị nông nghiệp', 1, NOW(), NOW()),
(3, 'Phòng Trừ Sâu Bệnh', 'Các biện pháp phòng trừ sâu bệnh thân thiện với môi trường', 1, NOW(), NOW());

-- Insert Forum Posts
INSERT INTO forum_posts (id, forum_category_id, user_id, title, slug, content, tags, view_count, like_count, dislike_count, is_pinned, status, created_at, updated_at) VALUES
(1, 1, 9, 'Kinh nghiệm trồng lúa hữu cơ tại Đồng Nai', 'kinh-nghiem-trong-lua-huu-co-tai-dong-nai', '[{"order": 1, "type": "text", "content": "Chào mọi người, mình đang trồng lúa hữu cơ tại Đồng Nai. Ai có kinh nghiệm chia sẻ nhé!"}]', 'lúa, hữu cơ, đồng nai', 150, 20, 2, 1, 'visible', NOW(), NOW()),
(2, 2, 7, 'Review máy cày mini điện VerdantTech V1', 'review-may-cay-mini-dien-verdanttech-v1', '[{"order": 1, "type": "text", "content": "Mình mới mua máy cày mini điện V1, chạy rất êm và tiết kiệm. Có ai dùng chưa?"}]', 'máy cày, điện, verdanttech', 80, 15, 1, 0, 'visible', NOW(), NOW()),
(3, 3, 10, 'Biện pháp phòng sâu bệnh tự nhiên cho rau củ', 'bien-phap-phong-sau-benh-tu-nhien-cho-rau-cu', '[{"order": 1, "type": "text", "content": "Mọi người thường dùng gì để phòng sâu bệnh cho rau mà không dùng thuốc hóa học?"}]', 'sâu bệnh, rau củ, tự nhiên', 120, 18, 0, 0, 'visible', NOW(), NOW());

-- Insert Forum Comments
INSERT INTO forum_comments (id, forum_post_id, user_id, parent_id, content, like_count, dislike_count, status, created_at, updated_at) VALUES
(1, 1, 10, NULL, 'Mình ở Long An cũng trồng lúa hữu cơ. Quan trọng là chọn giống lúa phù hợp không?', 3, 0, 'visible', NOW(), NOW()),
(2, 1, 9, 1, 'Mình thường chọn giống lúa ST24 hoặc ST25 vì phù hợp với đất phù sa và có chất lượng gạo tốt.', 5, 0, 'visible', NOW(), NOW()),
(3, 1, 7, NULL, 'Bài viết rất hữu ích! Mình đang cân nhắc chuyển từ canh tác truyền thống sang hữu cơ.', 2, 0, 'visible', NOW(), NOW()),
(4, 2, 17, NULL, 'Cảm ơn bạn đã đánh giá sản phẩm của chúng tôi! Nếu có bất kỳ thắc mắc nào về sử dụng, hãy liên hệ.', 4, 0, 'visible', NOW(), NOW()),
(5, 2, 10, 4, 'Máy chạy rất ổn, chỉ có điều pin hơi nhanh hết khi làm đất cứng.', 1, 0, 'visible', NOW(), NOW()),
(6, 3, 9, NULL, 'Bạn có thể thử dùng dung dịch tỏi ớt để xịt phòng trừ sâu bệnh.', 6, 0, 'visible', NOW(), NOW());

-- Insert Chatbot Conversations
INSERT INTO chatbot_conversations (id, customer_id, session_id, title, context, is_active, started_at) VALUES
(1, 7, 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'Tư vấn chọn máy cày', '{"topic": "equipment_consultation"}', 0, NOW()),
(2, 9, 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'Hỗ trợ kỹ thuật canh tác', '{"topic": "farming_techniques"}', 1, NOW()),
(3, 8, 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Tư vấn phân bón hữu cơ', '{"topic": "fertilizer_consultation"}', 0, NOW());

-- Insert Chatbot Messages
INSERT INTO chatbot_messages (id, conversation_id, message_type, message_text, created_at) VALUES
(1, 1, 'user', '{"text": "Tôi cần tư vấn chọn máy cày cho ruộng nhỏ khoảng 2ha"}', NOW()),
(2, 1, 'bot', '{"text": "Với diện tích 2ha, tôi khuyên bạn nên chọn máy cày mini điện VerdantTech V1."}', NOW()),
(3, 1, 'user', '{"text": "Giá của máy này là bao nhiêu? Có khuyến mãi không?"}', NOW()),
(4, 1, 'bot', '{"text": "Máy cày mini điện VerdantTech V1 có giá 5.000 VNĐ."}', NOW()),
(5, 2, 'user', '{"text": "Tôi muốn hỏi về kỹ thuật trồng lúa hữu cơ"}', NOW()),
(6, 2, 'bot', '{"text": "Trồng lúa hữu cơ cần chú ý những điểm sau: 1) Chuẩn bị đất 2) Chọn giống 3) Quản lý nước."}', NOW()),
(7, 3, 'user', '{"text": "Tôi trồng rau, đất cát, nên dùng loại phân nào?"}', NOW()),
(8, 3, 'bot', '{"text": "Với đất cát trồng rau, tôi khuyên bạn sử dụng Phân Compost Hữu Cơ Premium."}', NOW()),
(9, 3, 'user', '{"text": "Cảm ơn bạn! Tôi sẽ đặt mua ngay."}', NOW()),
(10, 3, 'bot', '{"text": "Rất vui được hỗ trợ bạn!"}', NOW());

`;
}

// =====================================================
// MAIN EXECUTION
// =====================================================

console.log('🚀 VerdantTech SEEDER Generator Started...\n');
console.log(`📋 Configuration:`);
console.log(`   - Max weight: ${MAX_WEIGHT_KG}kg`);
console.log(`   - Stock quantity: ${STOCK_QUANTITY}`);
console.log(`   - Batch quantity: ${BATCH_QUANTITY}`);
console.log(`   - Serials per product: ${SERIALS_PER_PRODUCT}`);
console.log(`   - Orders date range: ${ORDER_START_DATE.toISOString().slice(0, 10)} to ${ORDER_END_DATE.toISOString().slice(0, 10)}\n`);

const products = parseCSV();
const vendorDataSQL = generateVendorData();
const vendorCertsSQL = generateVendorCertificates();
const { sql: categoriesSQL, categoryMap } = generateCategories(products);
const productsSQL = generateProducts(products, categoryMap);
const { sql: productMediaSQL, lastMediaId } = generateProductMediaLinks(products);
const productCertsSQL = generateProductCertificates(products, lastMediaId);
const batchInventorySQL = generateBatchInventory(products);
const { sql: ordersSQL, orderDetails, orders } = generateOrders(products);
const reviewsSQL = generateProductReviews(orderDetails, orders);
const oldDataSQL = getOldSEEDERParts();

// Read base SEEDER (with farms, users, etc.)
const baseSEEDER = fs.readFileSync('SEEDER_BACKUP.sql', { encoding: 'utf8' });

// Combine all SQL in correct order
let finalSQL = baseSEEDER;
finalSQL += vendorDataSQL;          // 5. Vendor Profiles, Bank Accounts, Wallets
finalSQL += vendorCertsSQL;         // Vendor Certificates + Media
finalSQL += categoriesSQL;          // Product Categories
finalSQL += productsSQL;            // Products
finalSQL += productMediaSQL;        // Product Media Links
finalSQL += productCertsSQL;        // Product Certificates + Media
finalSQL += batchInventorySQL;      // Batch Inventory + Serials
finalSQL += ordersSQL;              // Orders, Order Details, Transactions, Payments, Export Inventory
finalSQL += reviewsSQL;             // Product Reviews
finalSQL += oldDataSQL;             // Forum + Chatbot

// Write to file
fs.writeFileSync('SEEDER.sql', finalSQL, 'utf8');

console.log('\n✅ SEEDER.sql generated successfully!');
console.log(`📊 Total lines: ${finalSQL.split('\n').length}`);
console.log(`📦 Total products: ${products.length}`);

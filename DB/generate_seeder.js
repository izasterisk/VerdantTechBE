// -*- coding: utf-8 -*-
/**
 * Script to generate complete SEEDER SQL for VerdantTech
 * Generates: 20 Vendors, 37 Categories, 172 Products, Certificates, Media, Inventory
 * Node.js version: 14+
 */

const fs = require('fs');
const path = require('path');

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
    let dimensions = {length: 20, width: 15, height: 10};
    
    // Phân bón (nặng)
    if (productName.includes('phân') || productName.includes('Phân')) {
        if (productName.includes('50kg')) {
            weight = 50.0;
            dimensions = {length: 80, width: 50, height: 20};
        } else if (productName.includes('20kg')) {
            weight = 20.0;
            dimensions = {length: 60, width: 40, height: 15};
        } else if (productName.includes('10kg')) {
            weight = 10.0;
            dimensions = {length: 50, width: 30, height: 15};
        } else if (productName.includes('5kg')) {
            weight = 5.0;
            dimensions = {length: 40, width: 25, height: 10};
        } else {
            weight = 25.0;
            dimensions = {length: 60, width: 40, height: 15};
        }
    }
    // Thuốc BVTV (nhẹ)
    else if (productName.includes('thuốc') || productName.includes('dầu')) {
        weight = 0.5;
        dimensions = {length: 15, width: 10, height: 8};
    }
    // Hạt giống (rất nhẹ)
    else if (productName.includes('Hạt giống')) {
        weight = 0.1;
        dimensions = {length: 12, width: 8, height: 2};
    }
    // Máy móc (rất nặng)
    else if (productName.includes('Máy')) {
        if (productName.includes('xới đất') || productName.includes('gặt')) {
            weight = 150.0;
            dimensions = {length: 180, width: 120, height: 100};
        } else if (productName.includes('bơm')) {
            weight = 35.0;
            dimensions = {length: 60, width: 40, height: 50};
        } else {
            weight = 15.0;
            dimensions = {length: 50, width: 35, height: 40};
        }
    }
    // Thiết bị điện (trung bình)
    else if (['Cảm biến', 'Relay', 'Bộ', 'Van'].some(k => productName.includes(k))) {
        weight = 0.8;
        dimensions = {length: 25, width: 20, height: 15};
    }
    // Ống/vật liệu cuộn (nhẹ nhưng lớn)
    else if (productName.includes('ống') || productName.includes('lưới') || productName.includes('Ống') || productName.includes('Lưới')) {
        if (productName.includes('100m')) {
            weight = 12.0;
            dimensions = {length: 100, width: 30, height: 30};
        } else {
            weight = 3.0;
            dimensions = {length: 50, width: 20, height: 20};
        }
    }
    // Dụng cụ cầm tay
    else if (['Cuốc', 'Xẻng', 'Kéo', 'Dao', 'Cào'].some(k => productName.includes(k))) {
        weight = 0.8;
        dimensions = {length: 35, width: 10, height: 5};
    }
    // Vật liệu nhẹ
    else if (['Túi', 'Hộp', 'Băng', 'Găng tay'].some(k => productName.includes(k))) {
        weight = 0.3;
        dimensions = {length: 30, width: 20, height: 5};
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

function isMachinery(productName) {
    const machineryKeywords = [
        'Máy xới', 'Máy phun thuốc', 'Máy phun sương', 'Máy phun phân', 'Máy gieo hạt',
        'Máy bơm nước', 'Máy bơm tăng áp', 'Máy bơm biến tần', 'Máy bơm chìm',
        'Đèn bẫy', 'Trạm thời tiết', 'Relay', 'Bộ điều khiển', 'Van điện từ'
    ];
    return machineryKeywords.some(k => productName.includes(k));
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
    sql += `INSERT INTO vendor_profiles (id, user_id, company_name, slug, business_registration_number, verified_at, verified_by, created_at, updated_at) VALUES\n`;
    
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
        vendorRows.push(`(${id}, ${userId}, '${name}', '${slug}', '${brn}', NOW(), 1, NOW(), NOW())`);
    });
    sql += vendorRows.join(',\n') + ';\n\n';
    
    // User Bank Accounts
    sql += `-- Insert User Bank Accounts (20 vendors - all same account 970436/1045069359)\n`;
    sql += `INSERT INTO user_bank_accounts (id, user_id, bank_code, account_number, is_active, created_at, updated_at) VALUES\n`;
    
    const bankRows = [];
    for (let i = 1; i <= 20; i++) {
        bankRows.push(`(${i}, ${16 + i}, '970436', '1045069359', 1, NOW(), NOW())`);
    }
    sql += bankRows.join(',\n') + ';\n\n';
    
    // Wallets
    sql += `-- Insert Wallets (20 vendors - starting balance 10,000,000 VND)\n`;
    sql += `INSERT INTO wallets (id, vendor_id, balance, last_updated_by, created_at, updated_at) VALUES\n`;
    
    const walletRows = [];
    for (let i = 1; i <= 20; i++) {
        walletRows.push(`(${i}, ${16 + i}, 10000000.00, 1, NOW(), NOW())`);
    }
    sql += walletRows.join(',\n') + ';\n\n';
    
    console.log('✅ Generated 20 vendor profiles, 20 bank accounts, 20 wallets');
    return sql;
}

function generateVendorCertificates() {
    const certificates = [
        {code: 'TCVN_7259', name: 'TCVN 7259:2003 - Phân hữu cơ'},
        {code: 'TCVN_8956', name: 'TCVN 8956:2011 - Phân vi sinh'},
        {code: 'ISO_14001', name: 'ISO 14001 Environmental Management'},
        {code: 'VIETGAP', name: 'VietGAP – Thực hành nông nghiệp tốt tại Việt Nam'},
        {code: 'USDA_ORGANIC', name: 'USDA Organic Certification'}
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
    let sql = `-- Insert Product Categories (11 parent + ${totalSubs} sub categories)\n`;
    sql += `INSERT INTO product_categories (id, parent_id, name, slug, description, is_active, created_at, updated_at) VALUES\n`;
    
    const rows = [];
    let catId = 1;
    
    // Parent categories
    for (const [key, cat] of categoryMap) {
        const slug = slugify(cat.name);
        rows.push(`(${catId}, NULL, '${escapeSQL(cat.name)}', '${slug}', 'Danh mục ${escapeSQL(cat.name)}', 1, NOW(), NOW())`);
        cat.dbId = catId;
        catId++;
    }
    
    // Sub categories
    for (const [key, cat] of categoryMap) {
        for (const [subKey, sub] of cat.subs) {
            const slug = slugify(sub.name);
            rows.push(`(${catId}, ${cat.dbId}, '${escapeSQL(sub.name)}', '${slug}', 'Danh mục con ${escapeSQL(sub.name)}', 1, NOW(), NOW())`);
            sub.dbId = catId;
            catId++;
        }
    }
    
    sql += rows.join(',\n') + ';\n\n';
    
    console.log(`✅ Generated ${rows.length} categories`);
    return {sql, categoryMap};
}

function generateProducts(products, categoryMap) {
    console.log('📦 Generating products...');
    
    let sql = `-- Insert Products (${products.length} products, price=1000, commission_rate=10%, stock=100)\n`;
    sql += `INSERT INTO products (id, category_id, vendor_id, product_code, product_name, slug, description, unit_price, commission_rate, discount_percentage, energy_efficiency_rating, specifications, manual_urls, public_url, warranty_months, stock_quantity, weight_kg, dimensions_cm, is_active, view_count, sold_count, rating_average, created_at, updated_at) VALUES\n`;
    
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
        if (p.subId === p.categoryId) {
            categoryDbId = cat.dbId;  // Gắn vào parent
        } else {
            // Có sub-category → tìm và dùng sub
            const sub = Array.from(cat.subs.values()).find(s => s.id === p.subId);
            if (!sub) {
                console.warn(`Warning: Sub-category ${p.subId} not found for product ${p.productName}`);
                return;
            }
            categoryDbId = sub.dbId;
        }
        const productCode = `PRD-C${p.categoryId.padStart(2, '0')}-${productId.toString().padStart(4, '0')}`;
        const slug = slugify(p.productName);
        const specs = generateSpecs(p.productName, p.categoryName);
        const {weight, dimensions} = generateWeightDimensions(p.productName);
        const warranty = getWarrantyMonths(p.productName);
        const energyRating = getEnergyEfficiencyRating(p.productName);
        
        // Rotate vendors every N products
        if (index > 0 && index % productsPerVendor === 0 && vendorId < 36) {
            vendorId++;
        }
        
        rows.push(`(${productId}, ${categoryDbId}, ${vendorId}, '${productCode}', '${escapeSQL(p.productName)}', '${slug}', 'Sản phẩm ${escapeSQL(p.productName)} chất lượng cao', 1000.00, 10.00, 0.00, ${energyRating}, "${specs}", NULL, NULL, ${warranty}, 100, ${weight}, "${dimensions}", 1, 0, 0, 0.00, NOW(), NOW())`);
        
        p.dbId = productId;
        p.vendorId = vendorId;
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
        
        // Image 1 (front)
        if (p.imageUrl1) {
            const publicId = p.imageUrl1.split('/').pop().split('.')[0];
            rows.push(`(${mediaId}, 'products', ${p.dbId}, '${escapeSQL(p.imageUrl1)}', '${publicId}', 'front', 0, NOW(), NOW())`);
            mediaId++;
        }
        
        // Image 2 (back)
        if (p.imageUrl2) {
            const publicId = p.imageUrl2.split('/').pop().split('.')[0];
            rows.push(`(${mediaId}, 'products', ${p.dbId}, '${escapeSQL(p.imageUrl2)}', '${publicId}', 'back', 1, NOW(), NOW())`);
            mediaId++;
        }
    });
    
    sql += rows.join(',\n') + ';\n\n';
    console.log(`✅ Generated ${rows.length} media links`);
    return sql;
}

function generateProductCertificates(products) {
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
    sql += `INSERT INTO product_certificates (id, product_id, certification_code, certification_name, status, rejection_reason, uploaded_at, verified_at, verified_by, created_at, updated_at) VALUES\n`;
    
    const rows = [];
    let certId = 1;
    let mediaId = rows.length + 21 + products.filter(p => p.imageUrl1 || p.imageUrl2).length * 2;
    
    products.forEach(p => {
        if (!p.dbId) return;
        
        const key = p.subId;
        if (certRequirements[key]) {
            const code = certRequirements[key];
            let name = '';
            
            switch(code) {
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
            
            rows.push(`(${certId}, ${p.dbId}, '${code}', '${name}', 'verified', NULL, NOW(), NOW(), 1, NOW(), NOW())`);
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
    
    let sql = `-- Insert Batch Inventory (${products.length} batches with quality check)\n`;
    sql += `INSERT INTO batch_inventory (id, product_id, sku, vendor_id, batch_number, lot_number, quantity, unit_cost_price, expiry_date, manufacturing_date, quality_check_status, quality_checked_by, quality_checked_at, notes, created_at, updated_at) VALUES\n`;
    
    const rows = [];
    products.forEach((p, index) => {
        if (!p.dbId) return;
        
        const batchId = p.dbId;
        const sku = `SKU-${p.dbId.toString().padStart(4, '0')}`;
        const batchNum = `BATCH${(index + 1).toString().padStart(4, '0')}`;
        const lotNum = `LOT${(index + 1).toString().padStart(4, '0')}`;
        const unitCost = '900.00'; // Cost = 900, selling price = 1000
        
        // Expiry date only for fertilizers, seeds, chemicals
        let expiryDate = 'NULL';
        if (p.categoryId === '1' || p.categoryId === '2' || p.categoryId === '3') {
            expiryDate = "'2026-12-31'";
        }
        
        rows.push(`(${batchId}, ${p.dbId}, '${sku}', ${p.vendorId}, '${batchNum}', '${lotNum}', 100, ${unitCost}, ${expiryDate}, '2025-01-01', 'passed', 2, NOW(), 'Nhập kho đợt đầu', NOW(), NOW())`);
    });
    
    sql += rows.join(',\n') + ';\n\n';
    
    // Product serials for machinery only
    const machineryProducts = products.filter(p => isMachinery(p.productName));
    
    if (machineryProducts.length > 0) {
        sql += `-- Insert Product Serials for Machinery (${machineryProducts.length} products x 10 units = ${machineryProducts.length * 10} serials)\n`;
        sql += `INSERT INTO product_serials (id, batch_inventory_id, product_id, serial_number, status, created_at, updated_at) VALUES\n`;
        
        const serialRows = [];
        let serialId = 1;
        
        machineryProducts.forEach(p => {
            if (!p.dbId) return;
            
            // Generate 10 serial numbers per machinery product
            for (let i = 1; i <= 10; i++) {
                const serialNum = `SN-${p.dbId.toString().padStart(4, '0')}-${i.toString().padStart(3, '0')}`;
                const status = i <= 2 ? 'sold' : 'stock'; // First 2 are sold
                serialRows.push(`(${serialId}, ${p.dbId}, ${p.dbId}, '${serialNum}', '${status}', NOW(), NOW())`);
                serialId++;
            }
        });
        
        sql += serialRows.join(',\n') + ';\n\n';
        console.log(`✅ Generated ${serialRows.length} product serials`);
    }
    
    console.log(`✅ Generated ${rows.length} batch inventory records`);
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
(1, 1, 'user', 'Tôi cần tư vấn chọn máy cày cho ruộng nhỏ khoảng 2ha', NOW()),
(2, 1, 'bot', 'Với diện tích 2ha, tôi khuyên bạn nên chọn máy cày mini điện VerdantTech V1.', NOW()),
(3, 1, 'user', 'Giá của máy này là bao nhiêu? Có khuyến mãi không?', NOW()),
(4, 1, 'bot', 'Máy cày mini điện VerdantTech V1 có giá 1.000 VNĐ.', NOW()),
(5, 2, 'user', 'Tôi muốn hỏi về kỹ thuật trồng lúa hữu cơ', NOW()),
(6, 2, 'bot', 'Trồng lúa hữu cơ cần chú ý những điểm sau: 1) Chuẩn bị đất 2) Chọn giống 3) Quản lý nước.', NOW()),
(7, 3, 'user', 'Tôi trồng rau, đất cát, nên dùng loại phân nào?', NOW()),
(8, 3, 'bot', 'Với đất cát trồng rau, tôi khuyên bạn sử dụng Phân Compost Hữu Cơ Premium.', NOW()),
(9, 3, 'user', 'Cảm ơn bạn! Tôi sẽ đặt mua ngay.', NOW()),
(10, 3, 'bot', 'Rất vui được hỗ trợ bạn!', NOW());

`;
}

// =====================================================
// MAIN EXECUTION
// =====================================================

console.log('🚀 VerdantTech SEEDER Generator Started...\n');

const products = parseCSV();
const vendorDataSQL = generateVendorData();
const vendorCertsSQL = generateVendorCertificates();
const {sql: categoriesSQL, categoryMap} = generateCategories(products);
const productsSQL = generateProducts(products, categoryMap);
const productMediaSQL = generateProductMediaLinks(products);
const productCertsSQL = generateProductCertificates(products);
const batchInventorySQL = generateBatchInventory(products);
const oldDataSQL = getOldSEEDERParts();

// Read base SEEDER (with farms, users, etc.)
const baseSEEDER = fs.readFileSync('SEEDER_BACKUP.sql', {encoding: 'utf8'});

// Combine all SQL in correct order
let finalSQL = baseSEEDER;
finalSQL += vendorDataSQL;          // 5. Vendor Profiles, Bank Accounts, Wallets
finalSQL += vendorCertsSQL;         // Vendor Certificates + Media
finalSQL += categoriesSQL;          // Product Categories
finalSQL += productsSQL;            // Products
finalSQL += productMediaSQL;        // Product Media Links
finalSQL += productCertsSQL;        // Product Certificates + Media
finalSQL += batchInventorySQL;      // Batch Inventory + Serials
finalSQL += oldDataSQL;             // Forum + Chatbot

// Write to file
fs.writeFileSync('SEEDER.sql', finalSQL, 'utf8');

console.log('\n✅ SEEDER.sql generated successfully!');
console.log(`📊 Total lines: ${finalSQL.split('\n').length}`);
console.log(`📦 Total products: ${products.length}`);


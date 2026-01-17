import os

input_file = "../PLU_1_20260107120410_part2.txt"
products_per_file = 500

current_product = []
product_count = 0
file_count = 1
all_products = []

with open(input_file, 'r', encoding='cp1254', errors='replace') as f:
    for line in f:
        line = line.rstrip('\n\r')
        
        if line.startswith('1;'):
            if current_product:
                all_products.append(current_product)
                product_count += 1
                
                if len(all_products) >= products_per_file:
                    out_file = f"products_{file_count:03d}_{(file_count-1)*products_per_file+1}_to_{file_count*products_per_file}.txt"
                    with open(out_file, 'w', encoding='cp1254') as out:
                        for prod in all_products:
                            for l in prod:
                                out.write(l + '\n')
                    print(f"Created: {out_file} ({len(all_products)} products)")
                    all_products = []
                    file_count += 1
            
            current_product = [line]
        else:
            current_product.append(line)

if current_product:
    all_products.append(current_product)
    product_count += 1

if all_products:
    start = (file_count-1)*products_per_file+1
    end = start + len(all_products) - 1
    out_file = f"products_{file_count:03d}_{start}_to_{end}.txt"
    with open(out_file, 'w', encoding='cp1254') as out:
        for prod in all_products:
            for l in prod:
                out.write(l + '\n')
    print(f"Created: {out_file} ({len(all_products)} products)")

print(f"\nTotal products: {product_count}")
print(f"Total files: {file_count}")

ingredientes = ['pepperoni', 'piña', 'queso']

# Generar todas las combinaciones posibles de 1, 2 y 3 elementos
combinaciones = []
for n in [1, 2, 3]:
    from itertools import product
    combinaciones.extend(['_'.join(comb) for comb in product(ingredientes, repeat=n)])

# Primera parte: declaraciones de Sprite
for comb in combinaciones:
    print(f"public Sprite {comb}Sprite;")

print()

# Segunda parte: inicializaciones
print("{")
for i, comb in enumerate(combinaciones):
    if i < len(combinaciones) - 1:
        print(f'    {{ "{comb}", {comb}Sprite }},')
    else:
        print(f'    {{ "{comb}", {comb}Sprite }}')
print("},")
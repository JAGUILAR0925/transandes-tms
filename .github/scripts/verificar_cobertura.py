#!/usr/bin/env python3
"""Falla si la cobertura de líneas de los ensamblados críticos baja del umbral (constitución, principio III).

Uso: verificar_cobertura.py <directorio_resultados> <umbral> <Ensamblado> [<Ensamblado> ...]
Toma, por ensamblado, la mayor cobertura reportada entre los informes Cobertura de cada proyecto de prueba.
Un ensamblado sin líneas instrumentables (p. ej. aún sin código) se informa y no hace fallar.
"""
import glob
import sys
import xml.etree.ElementTree as ET

directorio, umbral, ensamblados = sys.argv[1], float(sys.argv[2]), sys.argv[3:]
mejor = {}
for informe in glob.glob(f"{directorio}/**/coverage.cobertura.xml", recursive=True):
    for paquete in ET.parse(informe).getroot().iter("package"):
        nombre = paquete.get("name")
        lineas = sum(1 for _ in paquete.iter("line"))
        if lineas:
            mejor[nombre] = max(mejor.get(nombre, 0.0), float(paquete.get("line-rate", "0")))

fallas = 0
for ensamblado in ensamblados:
    if ensamblado not in mejor:
        print(f"ℹ️  {ensamblado}: sin líneas instrumentables todavía; no se evalúa")
        continue
    cobertura = mejor[ensamblado]
    estado = "✅" if cobertura >= umbral else "❌"
    print(f"{estado} {ensamblado}: {cobertura:.1%} (mínimo {umbral:.0%})")
    if cobertura < umbral:
        print(f"::error::Cobertura de {ensamblado} ({cobertura:.1%}) por debajo de {umbral:.0%}")
        fallas += 1

sys.exit(1 if fallas else 0)

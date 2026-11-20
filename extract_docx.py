import zipfile
import xml.etree.ElementTree as ET
import os
import re

docx_path = r'd:\workfiles\项目文件\集晨\胜宏\打点机预下发入库接口_MarkingPrePutIn.docx'
out_path = r'D:\workfiles\胜宏立库\四厂WMS系统\PM1018\PM1018\marking_api_spec.txt'

NS = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}

def local_tag(tag):
    if '}' in tag:
        return tag.split('}', 1)[1]
    return tag

def text_from_element(elem):
    parts = []
    for e in elem.iter():
        t = local_tag(e.tag)
        if t == 't' and e.text:
            parts.append(e.text)
        elif t == 'tab':
            parts.append('\t')
        elif t in ('br', 'cr'):
            parts.append('\n')
    return ''.join(parts)

def extract_paragraph(p):
    return text_from_element(p).strip()

def extract_table(tbl):
    rows_out = []
    for tr in tbl.findall('.//w:tr', NS):
        cells = []
        for tc in tr.findall('./w:tc', NS):
            cell_parts = []
            for p in tc.findall('.//w:p', NS):
                pt = extract_paragraph(p)
                if pt:
                    cell_parts.append(pt)
            cells.append('\n'.join(cell_parts))
        rows_out.append('\t'.join(cells))
    return '\n'.join(rows_out)

with zipfile.ZipFile(docx_path, 'r') as z:
    xml = z.read('word/document.xml')

root = ET.fromstring(xml)
body = root.find('w:body', NS)
lines = []

if body is not None:
    for child in body:
        tag = local_tag(child.tag)
        if tag == 'p':
            t = extract_paragraph(child)
            lines.append(t if t else '')
        elif tag == 'tbl':
            lines.append('')
            lines.append(extract_table(child))
            lines.append('')

text = '\n'.join(lines)
text = re.sub(r'\n{3,}', '\n\n', text)

os.makedirs(os.path.dirname(out_path), exist_ok=True)
with open(out_path, 'w', encoding='utf-8') as f:
    f.write(text)

print(len(text))

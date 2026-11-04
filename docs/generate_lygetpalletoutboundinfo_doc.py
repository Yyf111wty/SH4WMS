# -*- coding: utf-8 -*-
"""Generate Word API doc for lygetpalletoutboundinfo."""
import os
from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor

OUTPUT = os.path.join(
    os.path.dirname(__file__),
    "备刀机_组盘出库托盘查询接口.docx",
)


def set_cell_shading(cell, fill="E7E6E6"):
    tc = cell._tc
    tc_pr = tc.get_or_add_tcPr()
    shd = tc_pr.find(qn("w:shd"))
    if shd is None:
        from docx.oxml import OxmlElement

        shd = OxmlElement("w:shd")
        tc_pr.append(shd)
    shd.set(qn("w:fill"), fill)


def add_table(doc, headers, rows):
    table = doc.add_table(rows=1, cols=len(headers))
    table.style = "Table Grid"
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    hdr = table.rows[0].cells
    for i, text in enumerate(headers):
        hdr[i].text = text
        set_cell_shading(hdr[i])
        for p in hdr[i].paragraphs:
            for run in p.runs:
                run.bold = True
                run.font.size = Pt(10)
    for row in rows:
        cells = table.add_row().cells
        for i, text in enumerate(row):
            cells[i].text = str(text)
            for p in cells[i].paragraphs:
                for run in p.runs:
                    run.font.size = Pt(10)
    doc.add_paragraph("")
    return table


def add_code_block(doc, text):
    p = doc.add_paragraph()
    run = p.add_run(text)
    run.font.name = "Consolas"
    run._element.rPr.rFonts.set(qn("w:eastAsia"), "Consolas")
    run.font.size = Pt(9)
    run.font.color.rgb = RGBColor(0x1F, 0x1F, 0x1F)
    p.paragraph_format.left_indent = Cm(0.5)
    p.paragraph_format.space_after = Pt(6)


def main():
    doc = Document()
    normal = doc.styles["Normal"]
    normal.font.name = "微软雅黑"
    normal._element.rPr.rFonts.set(qn("w:eastAsia"), "微软雅黑")
    normal.font.size = Pt(11)

    title = doc.add_heading("备刀机 — 小刀组盘出库托盘查询接口", level=0)
    title.alignment = WD_ALIGN_PARAGRAPH.CENTER

    meta = doc.add_paragraph()
    meta.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run = meta.add_run("接口地址：POST /api/wms/lygetpalletoutboundinfo\n文档版本：1.2    更新日期：2026-08-25")
    run.font.size = Pt(10)
    run.font.color.rgb = RGBColor(0x66, 0x66, 0x66)

    doc.add_heading("1. 概述", level=1)
    doc.add_paragraph(
        "小刀组盘出库完成后，备刀机扫描大托盘条码（或单个小托盘条码），调用本接口向 WMS 查询该托盘内的"
        "盒子（小托盘）明细及对应出库任务的 workname（工单名称）。"
    )
    add_table(
        doc,
        ["项目", "说明"],
        [
            ["接口名称", "组盘出库托盘信息查询"],
            ["请求方式", "POST"],
            ["Content-Type", "application/json"],
            ["接口地址", "{WMS_BillApi基地址}/api/wms/lygetpalletoutboundinfo"],
        ],
    )
    doc.add_paragraph("说明：{WMS_BillApi基地址} 以现场部署为准，例如 http://192.168.x.x:端口")

    doc.add_heading("2. 业务背景", level=1)
    for item in [
        "备刀机调用 putoutbound 申请小钻出库，WMS 生成出库任务并返回托盘列表。",
        "WCS 将各小托盘送至组盘台（出库口 1002），组盘完成后调用 ReportPackingComplete 上报大托盘号与小托盘列表。",
        "大托盘流转至备刀机后，备刀机调用本接口获取托盘内全部盒子信息及工单名称 workname。",
    ]:
        doc.add_paragraph(item, style="List Number")

    doc.add_heading("3. 请求参数", level=1)
    add_table(
        doc,
        ["参数名", "类型", "必填", "说明"],
        [
            ["pallet", "string", "二选一", "大托盘条码（组盘完成上报的 pallet）"],
            ["container", "string", "二选一", "小托盘/盒子条码（按单盒反查所属托盘及同盘其他盒子）"],
        ],
    )
    doc.add_paragraph("说明：pallet 与 container 至少传一项；优先使用 pallet 查询整盘信息。")

    doc.add_heading("3.1 请求示例（按大托盘查询）", level=2)
    add_code_block(doc, '{\n  "pallet": "WHP16036"\n}')

    doc.add_heading("3.2 请求示例（按小托盘查询）", level=2)
    add_code_block(doc, '{\n  "container": "WH237375"\n}')

    doc.add_heading("4. 响应参数", level=1)
    add_table(
        doc,
        ["字段", "类型", "说明"],
        [
            ["Success", "bool", "是否成功"],
            ["Code", "int", "0=成功，1=失败"],
            ["Message", "string", "提示信息"],
            ["Result", "object", "业务数据，失败时为 null"],
        ],
    )

    doc.add_heading("4.1 Result 对象", level=2)
    add_table(
        doc,
        ["字段", "类型", "说明"],
        [
            ["workname", "string", "出库任务工单名称（与 putoutbound 请求中的 workname 一致）"],
            ["orderId", "string", "订单号"],
            ["pallet", "string", "大托盘条码"],
            ["boxCount", "int", "托盘内盒子数量"],
            ["boxes", "array", "盒子明细列表"],
        ],
    )

    doc.add_heading("4.2 boxes[] 元素", level=2)
    add_table(
        doc,
        ["字段", "类型", "说明"],
        [
            ["container", "string", "小托盘/盒子条码"],
            ["specc", "string", "规格编码"],
        ],
    )

    doc.add_heading("5. 响应示例", level=1)

    doc.add_heading("5.1 成功", level=2)
    add_code_block(
        doc,
        """{
  "Success": true,
  "Code": 0,
  "Message": "查询成功",
  "Result": {
    "workname": "20260811220854394",
    "orderId": "20260811220854394",
    "pallet": "WHP16036",
    "boxCount": 12,
    "boxes": [
      {
        "container": "WH237375",
        "specc": "CX_UC-0.300B/5.5*M0"
      }
    ]
  }
}""",
    )

    doc.add_heading("5.2 未查到数据", level=2)
    add_code_block(
        doc,
        """{
  "Success": false,
  "Code": 1,
  "Message": "未查询到组盘出库数据",
  "Result": null
}""",
    )

    doc.add_heading("5.3 参数缺失", level=2)
    add_code_block(
        doc,
        """{
  "Success": false,
  "Code": 1,
  "Message": "参数 pallet 或 container 至少填写一项",
  "Result": null
}""",
    )

    doc.add_heading("6. 调用建议", level=1)
    for item in [
        "推荐时机：WCS 组盘完成（ReportPackingComplete）且大托盘到达备刀机后，扫描大托盘条码调用本接口。",
        "查询范围：仅返回小刀组盘出库任务（TYPE=2，目标出库口 ENDTAXI=1002，状态 STATE>=2）。",
        "托盘复用：同一托盘条码可能多次参与组盘，接口按 LASTEDITTIME 取最新一次组盘批次（以 PALLET + ORDERINFO 标识），不会混入历史记录。",
        "workname 用途：备刀机可根据 Result.workname 关联本地工单/加工程序。",
        "重试：若组盘刚完成立即查询无数据，可间隔 1～2 秒重试（等待 WCS 上报落库）。",
        "编码：请求与响应均为 UTF-8 JSON。",
    ]:
        doc.add_paragraph(item, style="List Number")

    doc.add_heading("7. 相关接口", level=1)
    add_table(
        doc,
        ["接口", "说明"],
        [
            ["POST /api/wms/putoutbound", "备刀机申请小钻出库"],
            ["POST /api/wms/ReportPackingComplete", "WCS 组盘完成上报（WCS 调用）"],
            ["POST /api/wms/lygetcodelist", "按条码批量查询规格"],
        ],
    )

    doc.add_heading("8. 变更记录", level=1)
    add_table(
        doc,
        ["版本", "日期", "说明"],
        [
            ["1.0", "2026-08-24", "初版发布"],
            ["1.1", "2026-08-24", "托盘复用时仅返回最新一次组盘记录"],
            ["1.2", "2026-08-25", "boxes 精简为 container、specc"],
        ],
    )

    doc.save(OUTPUT)
    print(OUTPUT)


if __name__ == "__main__":
    main()

"""Testing plan markdown -> print HTML (IP1 print style) -> PDF via headless Chrome.
Run from anywhere: python3 ip2a/build-plan-pdf.py  (weasyprint is broken on this machine; see CLAUDE.md)."""
import pathlib, re, subprocess, markdown

here = pathlib.Path(__file__).resolve().parent
md = (here / "2026-09-25-ip2a-testing-plan.md").read_text()
title, body = md.split("\n", 1)
body_html = markdown.markdown(body, extensions=["tables", "sane_lists"])

css = re.search(r"<style>(.*?)</style>", (here.parent / "ip1" / "testing-plan-print.html").read_text(), re.S).group(1)
css += """
  code{ font-family:Menlo,monospace; font-size:8.4pt; background:var(--tint); padding:0 2px; }
  strong{ color:inherit; }
  a{ color:var(--uq); text-decoration:none; }
  table{ page-break-inside:avoid; break-inside:avoid; }
"""
html = f"""<!doctype html><html lang="en"><head><meta charset="utf-8">
<title>XR Renovation Previewer — IP2a Testing Plan</title><style>{css}</style></head>
<body><h1>{title.lstrip('# ').strip()}</h1>{body_html}</body></html>"""
out_html = here / "testing-plan-print.html"
out_html.write_text(html)

pdf = here / "IP2a-Testing-Plan-Kaike-Nehme.pdf"
subprocess.run(["/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", "--headless=new", "--disable-gpu",
                "--no-pdf-header-footer", "--virtual-time-budget=3000", f"--print-to-pdf={pdf}", out_html.as_uri()],
               check=True, capture_output=True)
print(pdf)

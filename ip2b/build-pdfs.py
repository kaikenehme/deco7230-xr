"""IP2b Blackboard PDFs: testing plan (with the research paragraph) and the AI acknowledgement.
Markdown -> print HTML (IP1 print styles) -> PDF via headless Chrome (weasyprint is broken here; see CLAUDE.md).
Run from anywhere:  python3 ip2b/build-pdfs.py
Re-run after every edit to the .md files. Warns if a [TODO ...] placeholder is still in either source."""
import pathlib, re, subprocess, sys, markdown

here = pathlib.Path(__file__).resolve().parent
ip1 = here.parent / "ip1"
CHROME = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"

EXTRA_CSS = """
  code{ font-family:Menlo,monospace; font-size:8.4pt; background:var(--tint); padding:0 2px; }
  strong{ color:inherit; }
  a{ color:var(--uq); text-decoration:none; }
  table{ page-break-inside:auto; break-inside:auto; }
  tr{ page-break-inside:avoid; break-inside:avoid; }
  thead{ display:table-header-group; }
"""
ACK_COLS = """
  table{ table-layout:fixed; }
  th:nth-child(1){ width:12%; } th:nth-child(2){ width:34%; } th:nth-child(3){ width:24%; }
  th:nth-child(4){ width:19%; } th:nth-child(5){ width:11%; }
  footer{ margin-top:12px; padding-top:6px; border-top:1px solid var(--rule); font-size:8.4pt; color:var(--muted); }
"""


def css_from(html_file):
    return re.search(r"<style>(.*?)</style>", html_file.read_text(), re.S).group(1)


def build(src, css, html_name, pdf_name, page_title, footer=""):
    md = (here / src).read_text()
    title, body = md.split("\n", 1)
    body_html = markdown.markdown(body, extensions=["tables", "sane_lists", "nl2br"])
    html = f"""<!doctype html><html lang="en"><head><meta charset="utf-8">
<title>{page_title}</title><style>{css}</style></head>
<body><h1>{title.lstrip('# ').strip()}</h1>{body_html}{footer}</body></html>"""
    out_html = here / html_name
    out_html.write_text(html)
    pdf = here / pdf_name
    subprocess.run([CHROME, "--headless=new", "--disable-gpu", "--no-pdf-header-footer",
                    "--virtual-time-budget=3000", f"--print-to-pdf={pdf}", out_html.as_uri()],
                   check=True, capture_output=True)
    todos = re.findall(r"\[TODO[^\]]*\]", md)
    print(pdf)
    for t in todos:
        print(f"  ⚠️  still in {src}: {t}", file=sys.stderr)
    return not todos


ok = build("2026-10-23-ip2b-testing-plan.md",
           css_from(ip1 / "testing-plan-print.html") + EXTRA_CSS,
           "testing-plan-print.html", "IP2b-Testing-Plan-Kaike-Nehme.pdf",
           "XR Renovation Previewer — IP2b Testing Plan")
ok &= build("ai-acknowledgement.md",
            css_from(ip1 / "ai-acknowledgement-print.html") + EXTRA_CSS + ACK_COLS,
            "ai-acknowledgement-print.html", "IP2b-AI-Acknowledgement-Kaike-Nehme.pdf",
            "AI acknowledgement — DECO7230 Interactive Prototype 2b",
            "<footer>DECO7230 Interactive Prototype 2b · Kaike Nehme · AI acknowledgement per UQ Library guidance "
            "(secure assessment) · source: <code>ip2b/ai-acknowledgement.md</code></footer>")
sys.exit(0 if ok else 1)

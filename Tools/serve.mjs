// Serveur local du build Web, avec les en-têtes Brotli attendus par Unity.
// Usage : node Tools/serve.mjs [dossier=Builds/Web] [port=8080]
import { createServer } from "node:http";
import { readFile, stat } from "node:fs/promises";
import { extname, join, normalize } from "node:path";

const root = process.argv[2] ?? "Builds/Web";
const port = Number(process.argv[3] ?? 8080);

const types = {
  ".html": "text/html; charset=utf-8", ".js": "application/javascript", ".wasm": "application/wasm",
  ".data": "application/octet-stream", ".json": "application/json", ".png": "image/png",
  ".css": "text/css", ".mp4": "video/mp4", ".ico": "image/x-icon",
};

createServer(async (req, res) => {
  let path = normalize(decodeURIComponent(req.url.split("?")[0])).replace(/^([/\\])+/, "");
  if (path.includes("..")) { res.writeHead(400).end(); return; }
  let file = join(root, path || "index.html");
  try {
    if ((await stat(file)).isDirectory()) file = join(file, "index.html");
    const headers = {};
    let ext = extname(file);
    if (ext === ".br" || ext === ".gz") {
      headers["Content-Encoding"] = ext === ".br" ? "br" : "gzip";
      ext = extname(file.slice(0, -ext.length));
    }
    headers["Content-Type"] = types[ext] ?? "application/octet-stream";
    res.writeHead(200, headers).end(await readFile(file));
  } catch {
    res.writeHead(404).end("introuvable");
  }
}).listen(port, () => console.log(`Build servi sur http://localhost:${port}`));

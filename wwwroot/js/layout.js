// =========================================================
// Diário Roxo! — cabeçalho, rodapé e proteção de páginas
// =========================================================

const LOGO_SVG = `
<svg viewBox="0 0 40 40" aria-hidden="true">
  <rect x="7" y="4" width="26" height="32" rx="4" fill="#6b3fa0"/>
  <rect x="7" y="4" width="6" height="32" rx="2" fill="#3d1f63"/>
  <path d="M23 13.5c-1.6-2.6-6-1.6-5.4 1.8.4 2.4 5.4 5.7 5.4 5.7s5-3.3 5.4-5.7c.6-3.4-3.8-4.4-5.4-1.8z" fill="#e48db4"/>
  <path d="M17 26h12M17 30h9" stroke="#c9b6e4" stroke-width="1.6" stroke-linecap="round"/>
</svg>`;

function montarLayout() {
    const pagina = location.pathname.split("/").pop() || "index.html";
    const usuario = Sessao.usuario;

    const link = (href, texto) =>
        `<a href="${href}" class="${pagina === href ? "ativo" : ""}">${texto}</a>`;

    let areaConta = link("login.html", "Entrar");
    if (usuario) {
        areaConta = `
            ${Sessao.admin ? link("admin.html", "Painel") : ""}
            ${link("perfil.html", "Meu perfil")}
            <button type="button" class="link" id="botaoSair">Sair</button>`;
    }

    const topo = document.getElementById("topo");
    if (topo) {
        topo.className = "topo";
        topo.innerHTML = `
            <div class="container">
                <a href="index.html" class="logo">${LOGO_SVG}<span>Diário Roxo<em>!</em></span></a>
                <button class="botao-menu" type="button" aria-label="Abrir menu" aria-expanded="false">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"><path d="M4 7h16M4 12h16M4 17h10"/></svg>
                </button>
                <nav class="menu" aria-label="Navegação principal">
                    ${link("index.html", "Início")}
                    ${link("catalogo.html", "Catálogo")}
                    ${link("albuns.html", "Álbuns")}
                    <span class="separador"></span>
                    ${areaConta}
                </nav>
            </div>`;

        const botaoMenu = topo.querySelector(".botao-menu");
        const menu = topo.querySelector(".menu");
        botaoMenu.addEventListener("click", () => {
            const aberto = menu.classList.toggle("aberto");
            botaoMenu.setAttribute("aria-expanded", String(aberto));
        });

        const sair = document.getElementById("botaoSair");
        if (sair) {
            sair.addEventListener("click", () => {
                Sessao.sair();
                location.href = "index.html";
            });
        }
    }

    const rodape = document.getElementById("rodape");
    if (rodape) {
        rodape.className = "rodape";
        rodape.innerHTML = `
            <div class="container">
                <span><strong>Diário Roxo!</strong> · catálogo de fãs da discografia da Olivia Rodrigo · projeto acadêmico</span>
                <span class="mao">feito com roxo ♡</span>
            </div>`;
    }
}

// Redireciona para o login se não houver sessão.
function exigirLogin() {
    if (!Sessao.logado) {
        location.href = `login.html?voltar=${encodeURIComponent(location.pathname.split("/").pop() + location.search)}`;
        return false;
    }
    return true;
}

// Somente administradores (a API também bloqueia com 403).
function exigirAdmin() {
    if (!exigirLogin()) return false;
    if (!Sessao.admin) {
        location.href = "index.html";
        return false;
    }
    return true;
}

document.addEventListener("DOMContentLoaded", montarLayout);

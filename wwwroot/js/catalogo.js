// Catálogo: pesquisa por título/álbum, filtros por álbum e gênero, paginação.
const estado = {
    pagina: 1,
    tamanhoPagina: 15,
    termo: "",
    modo: "busca",          // busca | titulo | album
    idAlbum: parametro("idAlbum") || "",
    idGenero: parametro("idGenero") || ""
};

document.addEventListener("DOMContentLoaded", async () => {
    estado.termo = parametro("busca") || "";
    document.getElementById("busca").value = estado.termo;

    document.getElementById("formBusca").addEventListener("submit", e => {
        e.preventDefault();
        estado.termo = document.getElementById("busca").value.trim();
        estado.pagina = 1;
        carregarMusicas();
    });

    document.getElementById("modoBusca").addEventListener("click", e => {
        const botao = e.target.closest("[data-modo]");
        if (!botao) return;
        estado.modo = botao.dataset.modo;
        marcarAtivo("modoBusca", botao);
        if (estado.termo) { estado.pagina = 1; carregarMusicas(); }
    });

    await carregarFiltros();
    carregarMusicas();
});

function marcarAtivo(idGrupo, botao) {
    document.querySelectorAll(`#${idGrupo} .chip`).forEach(c => c.classList.toggle("ativo", c === botao));
}

async function carregarFiltros() {
    try {
        const [albuns, generos] = await Promise.all([api("/Album"), api("/Genero")]);
        montarChips("filtroAlbum", albuns.map(a => ({ id: a.idAlbum, nome: a.nome })), "idAlbum");
        montarChips("filtroGenero", generos.map(g => ({ id: g.idGenero, nome: g.nome })), "idGenero");
    } catch (e) {
        document.getElementById("erroCatalogo").textContent = e.message;
    }
}

function montarChips(idGrupo, itens, chave) {
    const grupo = document.getElementById(idGrupo);
    grupo.innerHTML = `<button type="button" class="chip" data-id="">Todos</button>` +
        itens.map(i => `<button type="button" class="chip" data-id="${i.id}">${esc(i.nome)}</button>`).join("");

    const atual = grupo.querySelector(`[data-id="${CSS.escape(String(estado[chave]))}"]`) || grupo.querySelector('[data-id=""]');
    atual.classList.add("ativo");

    grupo.addEventListener("click", e => {
        const botao = e.target.closest(".chip");
        if (!botao) return;
        estado[chave] = botao.dataset.id;
        estado.pagina = 1;
        marcarAtivo(idGrupo, botao);
        carregarMusicas();
    });
}

async function carregarMusicas() {
    const lista = document.getElementById("listaMusicas");
    const resumo = document.getElementById("resumo");
    const erro = document.getElementById("erroCatalogo");
    erro.textContent = "";
    lista.innerHTML = `<div class="carregando"></div>`;

    const params = new URLSearchParams({ pagina: estado.pagina, tamanhoPagina: estado.tamanhoPagina });
    if (estado.termo) params.set(estado.modo, estado.termo);
    if (estado.idAlbum) params.set("idAlbum", estado.idAlbum);
    if (estado.idGenero) params.set("idGenero", estado.idGenero);

    try {
        const dados = await api(`/Musica?${params}`);
        const { items, total } = dados;

        resumo.textContent = total === 1 ? "1 música encontrada" : `${total} músicas encontradas`;
        lista.innerHTML = items.length
            ? items.map(cardMusica).join("")
            : `<div class="vazio" style="grid-column:1/-1"><span class="mao">nada por aqui...</span>Nenhuma música combina com a pesquisa e os filtros.</div>`;

        montarPaginacao(total);
    } catch (e) {
        erro.textContent = e.message;
        lista.innerHTML = "";
        resumo.textContent = "";
        document.getElementById("paginacao").innerHTML = "";
    }
}

function montarPaginacao(total) {
    const nav = document.getElementById("paginacao");
    const paginas = Math.max(1, Math.ceil(total / estado.tamanhoPagina));
    if (paginas <= 1) { nav.innerHTML = ""; return; }

    nav.innerHTML = `
        <button class="botao secundario pequeno" ${estado.pagina <= 1 ? "disabled" : ""} data-ir="-1">← Anterior</button>
        <span>página ${estado.pagina} de ${paginas}</span>
        <button class="botao secundario pequeno" ${estado.pagina >= paginas ? "disabled" : ""} data-ir="1">Próxima →</button>`;

    nav.querySelectorAll("[data-ir]").forEach(b => b.addEventListener("click", () => {
        estado.pagina += Number(b.dataset.ir);
        carregarMusicas();
        window.scrollTo({ top: 0, behavior: "smooth" });
    }));
}

// Lista de álbuns com capa, data e a relação de músicas.
document.addEventListener("DOMContentLoaded", carregarAlbuns);

async function carregarAlbuns() {
    const lista = document.getElementById("listaAlbuns");
    try {
        const albuns = await api("/Album");
        if (!albuns.length) {
            lista.innerHTML = `<div class="vazio"><span class="mao">ainda em branco</span>Nenhum álbum cadastrado.</div>`;
            return;
        }

        lista.innerHTML = albuns.map((a, i) => `
            <article class="album-bloco papel fita ${i % 2 ? "lavanda" : ""}" id="album-${a.idAlbum}">
                <img src="${esc(a.capa || CAPA_PADRAO)}" alt="Capa de ${esc(a.nome)}" loading="lazy"
                     onerror="this.onerror=null;this.src='${CAPA_PADRAO}'">
                <div>
                    <h2>${esc(a.nome)}</h2>
                    <div class="meta">
                        <span class="selo">${esc(nomeTipo(a.tipoLancamento))}</span>
                        <span class="selo rosa">${formatarDataLonga(a.dataLancamento)}</span>
                        <span class="selo cinza">${a.totalMusicas} ${a.totalMusicas === 1 ? "faixa" : "faixas"} no catálogo</span>
                    </div>
                    <p>${esc(a.descricao || "")}</p>
                    ${a.musicas.length ? `
                    <ol class="faixas">
                        ${a.musicas.map(m => `
                            <li><a href="musica.html?id=${m.idMusica}">
                                <span class="num">${m.numeroFaixa}</span>
                                <span>${esc(m.titulo)}</span>
                                <span class="dur">${formatarDuracao(m.duracao)}</span>
                            </a></li>`).join("")}
                    </ol>` : `<p><em>Nenhuma música cadastrada neste álbum.</em></p>`}
                </div>
            </article>`).join("");

        if (location.hash) document.querySelector(location.hash)?.scrollIntoView();
    } catch (e) {
        lista.innerHTML = "";
        document.getElementById("erroAlbuns").textContent = e.message;
    }
}

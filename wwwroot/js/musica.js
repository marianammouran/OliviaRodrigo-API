// Detalhes de uma música: musica.html?id=1
document.addEventListener("DOMContentLoaded", carregarMusica);

async function carregarMusica() {
    const alvo = document.getElementById("detalheMusica");
    const erro = document.getElementById("erroMusica");
    const id = Number(parametro("id"));

    if (!Number.isInteger(id) || id < 1) {
        alvo.innerHTML = "";
        erro.textContent = "Música não informada.";
        return;
    }

    try {
        const m = await api(`/Musica/${id}`);
        const album = m.idAlbumNavigation;
        const genero = m.idGeneroNavigation;
        document.title = `${m.titulo} · Diário Roxo!`;

        alvo.innerHTML = `
            <div class="detalhe papel fita">
                <img class="capa-grande" src="${esc(capaDaMusica(m))}" alt="Capa de ${esc(album?.nome ?? "")}"
                     onerror="this.onerror=null;this.src='${CAPA_PADRAO}'">
                <div class="info">
                    <span class="selo rosa">faixa ${m.numeroFaixa} de ${esc(album?.nome ?? "")}</span>
                    <h1>${esc(m.titulo)}</h1>
                    <p style="color:var(--tinta-suave);margin:0">
                        <a href="albuns.html#album-${m.idAlbum}">${esc(album?.nome ?? "")}</a>
                        ${album ? `· ${esc(nomeTipo(album.tipoLancamento))}` : ""}
                    </p>

                    <dl class="ficha">
                        <div><dt>Álbum</dt><dd>${esc(album?.nome ?? "—")}</dd></div>
                        <div><dt>Gênero</dt><dd>${esc(genero?.nome ?? "—")}</dd></div>
                        <div><dt>Faixa</dt><dd>nº ${m.numeroFaixa}</dd></div>
                        <div><dt>Duração</dt><dd>${formatarDuracao(m.duracao)}</dd></div>
                        <div><dt>Lançamento</dt><dd>${formatarData(m.dataLancamento)}</dd></div>
                    </dl>

                    <p class="nota-diario">${esc(m.descricao || "Sem anotações extras sobre esta faixa... por enquanto.")}</p>

                    <div style="display:flex;gap:.6rem;flex-wrap:wrap">
                        <a class="botao" href="catalogo.html?idAlbum=${m.idAlbum}">Mais faixas de ${esc(album?.nome ?? "este álbum")}</a>
                        <a class="botao secundario" href="catalogo.html?idGenero=${m.idGenero}">Mais ${esc(genero?.nome ?? "do gênero")}</a>
                    </div>
                </div>
            </div>`;
    } catch (e) {
        alvo.innerHTML = e.status === 404
            ? `<div class="vazio"><span class="mao">página arrancada...</span>Essa música não existe no catálogo.</div>`
            : "";
        if (e.status !== 404) erro.textContent = e.message;
    }
}

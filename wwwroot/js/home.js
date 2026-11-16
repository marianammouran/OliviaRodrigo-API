// Página inicial: destaques de SOUR e GUTS + músicas aleatórias, tudo vindo da API.
document.addEventListener("DOMContentLoaded", carregarInicio);

async function carregarInicio() {
    const destaques = document.getElementById("destaques");
    const grade = document.getElementById("destaqueMusicas");
    const erro = document.getElementById("erroHome");

    try {
        const [albuns, musicas] = await Promise.all([api("/Album"), buscarTodasMusicas()]);

        document.getElementById("contagem").textContent =
            `${musicas.length} músicas · ${albuns.length} lançamentos catalogados ♡`;

        const principais = albuns.filter(a => ["SOUR", "GUTS"].includes(a.nome.toUpperCase()));
        const exibidos = principais.length ? principais : albuns.slice(0, 2);

        document.getElementById("heroCapas").innerHTML = exibidos
            .map(a => `<img src="${esc(a.capa || CAPA_PADRAO)}" alt="">`).join("");

        destaques.innerHTML = exibidos.map((a, i) => `
            <article class="destaque papel fita ${i % 2 ? "lavanda" : ""}">
                <img src="${esc(a.capa || CAPA_PADRAO)}" alt="Capa de ${esc(a.nome)}" onerror="this.src='${CAPA_PADRAO}'">
                <div>
                    <span class="selo rosa">${new Date(a.dataLancamento).getUTCFullYear()} · ${esc(nomeTipo(a.tipoLancamento))}</span>
                    <h3 style="font-size:1.7rem;margin-top:.5rem">${esc(a.nome)}</h3>
                    <p>${esc(a.descricao || "")}</p>
                    <a class="botao pequeno" href="catalogo.html?idAlbum=${a.idAlbum}">Ver as ${a.totalMusicas} faixas</a>
                </div>
            </article>`).join("") || `<p class="vazio">Nenhum álbum cadastrado ainda.</p>`;

        const sorteadas = [...musicas].sort(() => Math.random() - .5).slice(0, 8);
        grade.innerHTML = sorteadas.length
            ? sorteadas.map(cardMusica).join("")
            : `<div class="vazio"><span class="mao">página em branco...</span>Nenhuma música cadastrada.</div>`;
    } catch (e) {
        erro.textContent = e.message;
        destaques.innerHTML = "";
        grade.innerHTML = "";
    }
}

// =========================================================
// Painel administrativo — CRUD de músicas, álbuns, gêneros e usuários
// Todas as operações usam a API real; a API também bloqueia não-admins (403).
// =========================================================
const dados = { musicas: [], albuns: [], generos: [], usuarios: [] };
let abaAtual = "musicas";
let filtroTexto = "";

document.addEventListener("DOMContentLoaded", async () => {
    if (!exigirAdmin()) return;

    document.querySelectorAll(".abas-painel button").forEach(botao => {
        botao.addEventListener("click", () => {
            document.querySelectorAll(".abas-painel button").forEach(b => b.classList.toggle("ativo", b === botao));
            abaAtual = botao.dataset.aba;
            filtroTexto = "";
            desenhar();
        });
    });

    document.getElementById("cancelarDialogo").addEventListener("click", () => document.getElementById("dialogo").close());

    await recarregar();
});

async function recarregar() {
    const erro = document.getElementById("erroPainel");
    erro.textContent = "";
    try {
        const [musicas, albuns, generos, usuarios] = await Promise.all([
            buscarTodasMusicas(), api("/Album"), api("/Genero"), api("/Usuario")
        ]);
        Object.assign(dados, { musicas, albuns, generos, usuarios });

        document.getElementById("totalMusicas").textContent = musicas.length;
        document.getElementById("totalAlbuns").textContent = albuns.length;
        document.getElementById("totalGeneros").textContent = generos.length;
        document.getElementById("totalUsuarios").textContent = usuarios.length;
        desenhar();
    } catch (e) {
        if (e.status === 401) { exigirLogin(); return; }
        if (e.status === 403) { location.href = "index.html"; return; }
        erro.textContent = e.message;
        document.getElementById("conteudoPainel").innerHTML = "";
    }
}

// ---------- desenho das abas ----------
function desenhar() {
    const alvo = document.getElementById("conteudoPainel");
    const config = {
        musicas: { titulo: "Músicas", botao: "Nova música", tabela: tabelaMusicas, novo: () => formMusica() },
        albuns: { titulo: "Álbuns", botao: "Novo álbum", tabela: tabelaAlbuns, novo: () => formAlbum() },
        generos: { titulo: "Gêneros", botao: "Novo gênero", tabela: tabelaGeneros, novo: () => formGenero() },
        usuarios: { titulo: "Usuários", botao: "Novo usuário", tabela: tabelaUsuarios, novo: () => formUsuario() }
    }[abaAtual];

    alvo.innerHTML = `
        <div class="barra-acoes">
            <input type="search" id="filtroPainel" placeholder="Filtrar ${config.titulo.toLowerCase()}..." value="${esc(filtroTexto)}">
            <button class="botao" type="button" id="botaoNovo">+ ${config.botao}</button>
        </div>
        <div class="tabela-wrap">${config.tabela()}</div>`;

    const filtro = document.getElementById("filtroPainel");
    filtro.addEventListener("input", () => {
        filtroTexto = filtro.value;
        alvo.querySelector(".tabela-wrap").innerHTML = config.tabela();
        ligarBotoes();
    });
    document.getElementById("botaoNovo").addEventListener("click", config.novo);
    ligarBotoes();
}

function combina(...textos) {
    const termo = filtroTexto.trim().toLowerCase();
    return !termo || textos.some(t => String(t ?? "").toLowerCase().includes(termo));
}

function tabelaMusicas() {
    const linhas = dados.musicas
        .filter(m => combina(m.titulo, m.idAlbumNavigation?.nome, m.idGeneroNavigation?.nome))
        .map(m => `
            <tr>
                <td><img class="mini" src="${esc(capaDaMusica(m))}" alt="" onerror="this.src='${CAPA_PADRAO}'"></td>
                <td><strong>${esc(m.titulo)}</strong></td>
                <td>${esc(m.idAlbumNavigation?.nome)}</td>
                <td>${m.numeroFaixa}</td>
                <td><span class="selo">${esc(m.idGeneroNavigation?.nome)}</span></td>
                <td>${formatarDuracao(m.duracao)}</td>
                <td class="acoes">
                    <button class="botao secundario pequeno" data-acao="editar-musica" data-id="${m.idMusica}">Editar</button>
                    <button class="botao perigo pequeno" data-acao="excluir-musica" data-id="${m.idMusica}">Excluir</button>
                </td>
            </tr>`).join("");
    return tabela(["", "Título", "Álbum", "Faixa", "Gênero", "Duração", ""], linhas);
}

function tabelaAlbuns() {
    const linhas = dados.albuns
        .filter(a => combina(a.nome, a.tipoLancamento))
        .map(a => `
            <tr>
                <td><img class="mini" src="${esc(a.capa || CAPA_PADRAO)}" alt="" onerror="this.src='${CAPA_PADRAO}'"></td>
                <td><strong>${esc(a.nome)}</strong></td>
                <td>${esc(nomeTipo(a.tipoLancamento))}</td>
                <td>${formatarData(a.dataLancamento)}</td>
                <td>${a.totalMusicas}</td>
                <td class="acoes">
                    <button class="botao secundario pequeno" data-acao="editar-album" data-id="${a.idAlbum}">Editar</button>
                    <button class="botao perigo pequeno" data-acao="excluir-album" data-id="${a.idAlbum}">Excluir</button>
                </td>
            </tr>`).join("");
    return tabela(["", "Nome", "Tipo", "Lançamento", "Músicas", ""], linhas);
}

function tabelaGeneros() {
    const linhas = dados.generos
        .filter(g => combina(g.nome))
        .map(g => {
            const qtd = dados.musicas.filter(m => m.idGenero === g.idGenero).length;
            return `
            <tr>
                <td><strong>${esc(g.nome)}</strong></td>
                <td>${qtd}</td>
                <td class="acoes">
                    <button class="botao secundario pequeno" data-acao="editar-genero" data-id="${g.idGenero}">Editar</button>
                    <button class="botao perigo pequeno" data-acao="excluir-genero" data-id="${g.idGenero}">Excluir</button>
                </td>
            </tr>`;
        }).join("");
    return tabela(["Nome", "Músicas", ""], linhas);
}

function tabelaUsuarios() {
    const meuId = Sessao.usuario?.idUsuario;
    const linhas = dados.usuarios
        .filter(u => combina(u.nome, u.email, u.tipoUsuario))
        .map(u => `
            <tr>
                <td><strong>${esc(u.nome)}</strong>${u.idUsuario === meuId ? ' <span class="selo rosa">você</span>' : ""}</td>
                <td>${esc(u.email)}</td>
                <td><span class="selo ${u.tipoUsuario === "Admin" ? "rosa" : ""}">${u.tipoUsuario === "Admin" ? "Administrador" : "Usuário"}</span></td>
                <td>${formatarData(u.dataCadastro)}</td>
                <td><span class="selo ${u.ativo ? "" : "cinza"}">${u.ativo ? "Ativa" : "Desativada"}</span></td>
                <td class="acoes">
                    <button class="botao secundario pequeno" data-acao="editar-usuario" data-id="${u.idUsuario}">Editar</button>
                    ${u.idUsuario === meuId ? "" : u.ativo
                        ? `<button class="botao perigo pequeno" data-acao="desativar-usuario" data-id="${u.idUsuario}">Desativar</button>`
                        : `<button class="botao pequeno" data-acao="reativar-usuario" data-id="${u.idUsuario}">Reativar</button>`}
                </td>
            </tr>`).join("");
    return tabela(["Nome", "E-mail", "Perfil", "Cadastro", "Situação", ""], linhas);
}

function tabela(cabecalhos, linhas) {
    return `
        <table>
            <thead><tr>${cabecalhos.map(c => `<th>${c}</th>`).join("")}</tr></thead>
            <tbody>${linhas || `<tr><td colspan="${cabecalhos.length}" class="vazio">Nenhum registro encontrado.</td></tr>`}</tbody>
        </table>`;
}

function ligarBotoes() {
    document.querySelectorAll("#conteudoPainel [data-acao]").forEach(botao => {
        botao.addEventListener("click", () => executarAcao(botao.dataset.acao, Number(botao.dataset.id)));
    });
}

async function executarAcao(acao, id) {
    const acoes = {
        "editar-musica": () => formMusica(dados.musicas.find(m => m.idMusica === id)),
        "editar-album": () => formAlbum(dados.albuns.find(a => a.idAlbum === id)),
        "editar-genero": () => formGenero(dados.generos.find(g => g.idGenero === id)),
        "editar-usuario": () => formUsuario(dados.usuarios.find(u => u.idUsuario === id)),
        "excluir-musica": () => remover(`/Musica/${id}`, "Excluir esta música do catálogo?", "Música excluída."),
        "excluir-album": () => remover(`/Album/${id}`, "Excluir este álbum? (só é possível se ele não tiver músicas)", "Álbum excluído."),
        "excluir-genero": () => remover(`/Genero/${id}`, "Excluir este gênero? (só é possível se nenhuma música usar ele)", "Gênero excluído."),
        "desativar-usuario": () => remover(`/Usuario/${id}`, "Desativar esta conta? O usuário não conseguirá mais entrar.", "Usuário desativado."),
        "reativar-usuario": () => reativar(dados.usuarios.find(u => u.idUsuario === id))
    };
    await acoes[acao]?.();
}

async function remover(caminho, pergunta, sucesso) {
    if (!confirm(pergunta)) return;
    try {
        await api(caminho, { metodo: "DELETE" });
        toast(sucesso);
        await recarregar();
    } catch (e) {
        toast(e.message, "erro");
    }
}

async function reativar(u) {
    try {
        await api(`/Usuario/${u.idUsuario}`, {
            metodo: "PUT",
            corpo: { nome: u.nome, email: u.email, senha: null, tipoUsuario: u.tipoUsuario, ativo: true }
        });
        toast("Usuário reativado.");
        await recarregar();
    } catch (e) {
        toast(e.message, "erro");
    }
}

// ---------- diálogo genérico ----------
function abrirDialogo(titulo, camposHtml, aoSalvar, aoAbrir) {
    const dialogo = document.getElementById("dialogo");
    const form = document.getElementById("formDialogo");
    const erro = document.getElementById("erroDialogo");
    document.getElementById("tituloDialogo").textContent = titulo;
    document.getElementById("camposDialogo").innerHTML = camposHtml;
    erro.textContent = "";

    form.onsubmit = async e => {
        e.preventDefault();
        limparErros(form);
        erro.textContent = "";
        const corpo = aoSalvar(form);
        if (!corpo) return;

        const botao = form.querySelector("button[type=submit]");
        botao.disabled = true;
        try {
            await corpo.enviar();
            dialogo.close();
            toast(corpo.mensagem);
            await recarregar();
        } catch (falha) {
            erro.textContent = falha.message;
        } finally {
            botao.disabled = false;
        }
    };

    dialogo.showModal();
    aoAbrir?.(form);
    form.querySelector("input, select, textarea")?.focus();
}

const campo = (id, rotulo, html, extra = "") =>
    `<div class="campo ${extra}"><label for="${id}">${rotulo}</label>${html}<div class="erro-campo"></div></div>`;

const opcoes = (lista, valorId, texto, selecionado) =>
    `<option value="">Selecione...</option>` +
    lista.map(i => `<option value="${i[valorId]}" ${i[valorId] === selecionado ? "selected" : ""}>${esc(i[texto])}</option>`).join("");

// ---------- Música ----------
function formMusica(m) {
    const editando = Boolean(m);
    const html = `
        <div class="grade-form">
            ${campo("fTitulo", "Título", `<input id="fTitulo" maxlength="150" value="${esc(m?.titulo)}">`, "inteiro")}
            ${campo("fAlbum", "Álbum", `<select id="fAlbum">${opcoes(dados.albuns, "idAlbum", "nome", m?.idAlbum)}</select>`)}
            ${campo("fGenero", "Gênero", `<select id="fGenero">${opcoes(dados.generos, "idGenero", "nome", m?.idGenero)}</select>`)}
            ${campo("fFaixa", "Número da faixa", `<input id="fFaixa" type="number" min="1" max="999" value="${m?.numeroFaixa ?? ""}">`)}
            ${campo("fDuracao", "Duração (m:ss)", `<input id="fDuracao" placeholder="3:25" value="${m ? formatarDuracao(m.duracao) : ""}">`)}
            ${campo("fData", "Data de lançamento", `<input id="fData" type="date" value="${m?.dataLancamento ?? ""}">`)}
            ${campo("fImagem", "Imagem (opcional)", `<input id="fImagem" maxlength="500" placeholder="usa a capa do álbum se vazio" value="${esc(m?.imagemCapa)}">`)}
            ${campo("fDescricao", "Informações adicionais", `<textarea id="fDescricao" maxlength="1000">${esc(m?.descricao)}</textarea>`, "inteiro")}
        </div>`;

    abrirDialogo(editando ? "Editar música" : "Nova música", html, form => {
        const titulo = form.querySelector("#fTitulo");
        const album = form.querySelector("#fAlbum");
        const genero = form.querySelector("#fGenero");
        const faixa = form.querySelector("#fFaixa");
        const duracao = form.querySelector("#fDuracao");
        const data = form.querySelector("#fData");
        const segundos = duracaoParaSegundos(duracao.value);

        const ok = [
            validarObrigatorio(titulo, "o título"),
            marcarErro(album, album.value ? "" : "Escolha o álbum."),
            marcarErro(genero, genero.value ? "" : "Escolha o gênero."),
            marcarErro(faixa, Number(faixa.value) >= 1 ? "" : "Informe um número de faixa válido."),
            marcarErro(duracao, segundos > 0 ? "" : "Use o formato m:ss (ex.: 3:25)."),
            marcarErro(data, data.value ? "" : "Informe a data.")
        ].every(Boolean);
        if (!ok) return null;

        const corpo = {
            titulo: titulo.value.trim(),
            idAlbum: Number(album.value),
            idGenero: Number(genero.value),
            numeroFaixa: Number(faixa.value),
            dataLancamento: data.value,
            duracao: segundos,
            imagemCapa: form.querySelector("#fImagem").value.trim() || null,
            descricao: form.querySelector("#fDescricao").value.trim() || null
        };
        return {
            enviar: () => api(editando ? `/Musica/${m.idMusica}` : "/Musica", { metodo: editando ? "PUT" : "POST", corpo }),
            mensagem: editando ? "Música atualizada." : "Música cadastrada."
        };
    }, form => {
        // Sugere a data do álbum ao escolher o álbum (se a data estiver vazia).
        form.querySelector("#fAlbum").addEventListener("change", e => {
            const data = form.querySelector("#fData");
            const album = dados.albuns.find(a => a.idAlbum === Number(e.target.value));
            if (album && !data.value) data.value = album.dataLancamento;
        });
    });
}

// ---------- Álbum ----------
function formAlbum(a) {
    const editando = Boolean(a);
    const tipos = ["Album", "EP", "Single", "Deluxe", "Outro"];
    const html = `
        <div class="grade-form">
            ${campo("fNome", "Nome", `<input id="fNome" maxlength="100" value="${esc(a?.nome)}">`, "inteiro")}
            ${campo("fTipo", "Tipo de lançamento", `<select id="fTipo">${tipos.map(t => `<option value="${t}" ${a?.tipoLancamento === t ? "selected" : ""}>${nomeTipo(t)}</option>`).join("")}</select>`)}
            ${campo("fData", "Data de lançamento", `<input id="fData" type="date" value="${a?.dataLancamento ?? ""}">`)}
            ${campo("fCapa", "Capa (URL ou caminho local)", `<input id="fCapa" maxlength="500" placeholder="img/capas/sour.svg ou https://..." value="${esc(a?.capa)}">`, "inteiro")}
            ${campo("fDescricao", "Descrição", `<textarea id="fDescricao" maxlength="1000">${esc(a?.descricao)}</textarea>`, "inteiro")}
        </div>`;

    abrirDialogo(editando ? "Editar álbum" : "Novo álbum", html, form => {
        const nome = form.querySelector("#fNome");
        const data = form.querySelector("#fData");
        const ok = [validarObrigatorio(nome, "o nome"), marcarErro(data, data.value ? "" : "Informe a data.")].every(Boolean);
        if (!ok) return null;

        const corpo = {
            nome: nome.value.trim(),
            tipoLancamento: form.querySelector("#fTipo").value,
            dataLancamento: data.value,
            capa: form.querySelector("#fCapa").value.trim() || null,
            descricao: form.querySelector("#fDescricao").value.trim() || null
        };
        return {
            enviar: () => api(editando ? `/Album/${a.idAlbum}` : "/Album", { metodo: editando ? "PUT" : "POST", corpo }),
            mensagem: editando ? "Álbum atualizado." : "Álbum cadastrado."
        };
    });
}

// ---------- Gênero ----------
function formGenero(g) {
    const editando = Boolean(g);
    const html = campo("fNome", "Nome do gênero", `<input id="fNome" maxlength="50" value="${esc(g?.nome)}">`);

    abrirDialogo(editando ? "Editar gênero" : "Novo gênero", html, form => {
        const nome = form.querySelector("#fNome");
        if (!validarObrigatorio(nome, "o nome")) return null;
        const corpo = { nome: nome.value.trim() };
        return {
            enviar: () => api(editando ? `/Genero/${g.idGenero}` : "/Genero", { metodo: editando ? "PUT" : "POST", corpo }),
            mensagem: editando ? "Gênero atualizado." : "Gênero cadastrado."
        };
    });
}

// ---------- Usuário ----------
function formUsuario(u) {
    const editando = Boolean(u);
    const ehVoce = editando && u.idUsuario === Sessao.usuario?.idUsuario;
    const html = `
        <div class="grade-form">
            ${campo("fNome", "Nome", `<input id="fNome" maxlength="100" value="${esc(u?.nome)}">`)}
            ${campo("fEmail", "E-mail", `<input id="fEmail" type="email" maxlength="100" value="${esc(u?.email)}">`)}
            ${campo("fSenha", editando ? "Nova senha (opcional)" : "Senha", `<input id="fSenha" type="password" autocomplete="new-password">`)}
            ${campo("fTipo", "Perfil de acesso", `<select id="fTipo" ${ehVoce ? "disabled" : ""}>
                <option value="Usuario" ${u?.tipoUsuario === "Usuario" ? "selected" : ""}>Usuário comum</option>
                <option value="Admin" ${u?.tipoUsuario === "Admin" ? "selected" : ""}>Administrador</option>
            </select>`)}
            ${editando && !ehVoce ? `<div class="campo inteiro"><label class="checkbox"><input type="checkbox" id="fAtivo" ${u.ativo ? "checked" : ""}> Conta ativa</label></div>` : ""}
        </div>`;

    abrirDialogo(editando ? "Editar usuário" : "Novo usuário", html, form => {
        const nome = form.querySelector("#fNome");
        const email = form.querySelector("#fEmail");
        const senha = form.querySelector("#fSenha");
        const ok = [validarObrigatorio(nome, "o nome"), validarEmail(email), validarSenha(senha, editando)].every(Boolean);
        if (!ok) return null;

        const tipo = form.querySelector("#fTipo").value;
        if (!editando) {
            const corpo = { nome: nome.value.trim(), email: email.value.trim(), senha: senha.value, tipoUsuario: tipo };
            return { enviar: () => api("/Usuario", { metodo: "POST", corpo }), mensagem: "Usuário cadastrado." };
        }

        const corpo = {
            nome: nome.value.trim(),
            email: email.value.trim(),
            senha: senha.value || null,
            tipoUsuario: ehVoce ? u.tipoUsuario : tipo,
            ativo: ehVoce ? true : form.querySelector("#fAtivo").checked
        };
        return {
            enviar: async () => {
                await api(`/Usuario/${u.idUsuario}`, { metodo: "PUT", corpo });
                if (ehVoce) Sessao.atualizarUsuario({ ...Sessao.usuario, nome: corpo.nome, email: corpo.email });
            },
            mensagem: "Usuário atualizado."
        };
    });
}

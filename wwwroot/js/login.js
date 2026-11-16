// Login e criação de conta (perfil "Usuario").
document.addEventListener("DOMContentLoaded", () => {
    if (Sessao.logado) {
        location.href = Sessao.admin ? "admin.html" : "perfil.html";
        return;
    }

    const formEntrar = document.getElementById("formEntrar");
    const formCadastrar = document.getElementById("formCadastrar");
    const mensagem = document.getElementById("mensagemLogin");

    document.querySelectorAll(".abas button").forEach(botao => {
        botao.addEventListener("click", () => {
            document.querySelectorAll(".abas button").forEach(b => b.classList.toggle("ativo", b === botao));
            const cadastrar = botao.dataset.aba === "cadastrar";
            formEntrar.hidden = cadastrar;
            formCadastrar.hidden = !cadastrar;
            mostrar("");
        });
    });

    function mostrar(texto, tipo = "") {
        mensagem.textContent = texto;
        mensagem.className = `aviso ${tipo}`;
    }

    async function entrar(email, senha) {
        const dados = await api("/Login", { metodo: "POST", corpo: { email, senha } });
        Sessao.salvar(dados);
        const voltar = parametro("voltar");
        location.href = voltar && /^[\w-]+\.html/.test(voltar)
            ? voltar
            : (dados.usuario.tipoUsuario === "Admin" ? "admin.html" : "catalogo.html");
    }

    formEntrar.addEventListener("submit", async e => {
        e.preventDefault();
        limparErros(formEntrar);
        const email = document.getElementById("emailLogin");
        const senha = document.getElementById("senhaLogin");
        const ok = [validarEmail(email), validarObrigatorio(senha, "a senha")].every(Boolean);
        if (!ok) return;

        const botao = formEntrar.querySelector("button[type=submit]");
        botao.disabled = true;
        mostrar("Entrando...");
        try {
            await entrar(email.value.trim(), senha.value);
        } catch (erro) {
            mostrar(erro.status === 401 ? (erro.message || "E-mail ou senha inválidos.") : erro.message, "erro");
        } finally {
            botao.disabled = false;
        }
    });

    formCadastrar.addEventListener("submit", async e => {
        e.preventDefault();
        limparErros(formCadastrar);
        const nome = document.getElementById("nomeCadastro");
        const email = document.getElementById("emailCadastro");
        const senha = document.getElementById("senhaCadastro");
        const confirma = document.getElementById("confirmaCadastro");

        let ok = [validarObrigatorio(nome, "o nome"), validarEmail(email), validarSenha(senha)].every(Boolean);
        if (senha.value !== confirma.value) ok = marcarErro(confirma, "As senhas não conferem.") && false;
        if (!ok) return;

        const botao = formCadastrar.querySelector("button[type=submit]");
        botao.disabled = true;
        mostrar("Criando sua conta...");
        try {
            await api("/Usuario", {
                metodo: "POST",
                corpo: { nome: nome.value.trim(), email: email.value.trim(), senha: senha.value }
            });
            mostrar("Conta criada! Entrando...", "sucesso");
            await entrar(email.value.trim(), senha.value);
        } catch (erro) {
            mostrar(erro.message, "erro");
        } finally {
            botao.disabled = false;
        }
    });
});

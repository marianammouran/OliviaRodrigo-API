// Perfil do usuário autenticado: visualizar e editar nome, e-mail e senha.
let perfilAtual = null;

document.addEventListener("DOMContentLoaded", () => {
    if (!exigirLogin()) return;
    carregarPerfil();
    document.getElementById("formPerfil").addEventListener("submit", salvarPerfil);
});

async function carregarPerfil() {
    try {
        perfilAtual = await api("/Usuario/perfil");
        Sessao.atualizarUsuario(perfilAtual);
        desenharCartao();
        document.getElementById("nomePerfil").value = perfilAtual.nome;
        document.getElementById("emailPerfil").value = perfilAtual.email;
    } catch (e) {
        if (e.status === 401) { exigirLogin(); return; }
        document.getElementById("erroPerfil").textContent = e.message;
        document.getElementById("cartaoPerfil").innerHTML = "";
    }
}

function desenharCartao() {
    const u = perfilAtual;
    document.getElementById("cartaoPerfil").innerHTML = `
        <div class="avatar">${esc(u.nome.trim().charAt(0).toUpperCase())}</div>
        <h2 style="margin-bottom:.2rem">${esc(u.nome)}</h2>
        <span class="selo ${u.tipoUsuario === "Admin" ? "rosa" : ""}">${u.tipoUsuario === "Admin" ? "Administrador" : "Usuário"}</span>
        <dl>
            <div><dt>E-mail</dt><dd>${esc(u.email)}</dd></div>
            <div><dt>Membro desde</dt><dd>${formatarDataLonga(u.dataCadastro)}</dd></div>
            <div><dt>Situação da conta</dt><dd>${u.ativo ? "Ativa" : "Desativada"}</dd></div>
        </dl>`;
}

async function salvarPerfil(e) {
    e.preventDefault();
    const form = e.target;
    limparErros(form);
    const msg = document.getElementById("mensagemPerfil");
    msg.className = "aviso"; msg.textContent = "";

    const nome = document.getElementById("nomePerfil");
    const email = document.getElementById("emailPerfil");
    const senha = document.getElementById("senhaPerfil");
    const confirma = document.getElementById("confirmaPerfil");

    let ok = [validarObrigatorio(nome, "o nome"), validarEmail(email), validarSenha(senha, false)].every(Boolean);
    if (senha.value && senha.value !== confirma.value) ok = marcarErro(confirma, "As senhas não conferem.") && false;
    if (!ok) return;

    const botao = form.querySelector("button[type=submit]");
    botao.disabled = true;
    try {
        await api(`/Usuario/${perfilAtual.idUsuario}`, {
            metodo: "PUT",
            corpo: { nome: nome.value.trim(), email: email.value.trim(), senha: senha.value || null }
        });
        senha.value = ""; confirma.value = "";
        msg.className = "aviso sucesso";
        msg.textContent = "Informações atualizadas!";
        await carregarPerfil();
    } catch (erro) {
        msg.className = "aviso erro";
        msg.textContent = erro.message;
    } finally {
        botao.disabled = false;
    }
}

// Validação simples de formulários (mensagens abaixo de cada campo).
const EMAIL_VALIDO = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function marcarErro(input, mensagem) {
    input.classList.toggle("invalido", Boolean(mensagem));
    const alvo = input.closest(".campo")?.querySelector(".erro-campo");
    if (alvo) alvo.textContent = mensagem || "";
    return !mensagem;
}

function limparErros(form) {
    form.querySelectorAll(".invalido").forEach(i => i.classList.remove("invalido"));
    form.querySelectorAll(".erro-campo").forEach(e => e.textContent = "");
}

function validarObrigatorio(input, nome) {
    return marcarErro(input, input.value.trim() ? "" : `Informe ${nome}.`);
}

function validarEmail(input) {
    const valor = input.value.trim();
    if (!valor) return marcarErro(input, "Informe o e-mail.");
    return marcarErro(input, EMAIL_VALIDO.test(valor) ? "" : "Digite um e-mail válido.");
}

function validarSenha(input, obrigatoria = true) {
    const valor = input.value;
    if (!valor && !obrigatoria) return marcarErro(input, "");
    if (!valor) return marcarErro(input, "Informe a senha.");
    return marcarErro(input, valor.length >= 8 ? "" : "A senha deve ter pelo menos 8 caracteres.");
}

// Limpa o parâmetro token da URL caso exista
const urlParams = new URLSearchParams(window.location.search);
if (urlParams.get('token')) {
    window.history.replaceState({}, document.title, window.location.pathname);
}

function terminarSessao() {
    window.location.href = 'https://localhost:7194/login.html';
}

function escaparHtml(valor) {
    return String(valor ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function mostrarTabelaVazia(tabelaId, numeroColunas, mensagem) {
    const tabela = document.getElementById(tabelaId);
    if (tabela) {
        tabela.innerHTML = `
            <tr>
                <td colspan="${numeroColunas}" class="empty-state" style="text-align: center; padding: 25px;">
                    ${escaparHtml(mensagem)}
                </td>
            </tr>`;
    }
}

function formatarMoeda(valor) {
    return `${Number(valor || 0).toFixed(2)} €`;
}

function formatarData(data) {
    return data
        ? new Date(data).toLocaleString('pt-PT')
        : 'Ainda não concluída';
}

// Máscara automática para matrículas (XX-XX-XX)
document.addEventListener('input', function (e) {
    if (e.target && (e.target.id === 'matriculaInput' || e.target.id === 'editMatriculaInput')) {
        let valor = e.target.value.replace(/[^a-zA-Z0-9]/g, '').toUpperCase();
        if (valor.length > 6) valor = valor.substring(0, 6);

        let formatada = '';
        for (let i = 0; i < valor.length; i++) {
            if (i === 2 || i === 4) formatada += '-';
            formatada += valor[i];
        }
        e.target.value = formatada;
    }
});

async function carregarNomeUtilizador() {
    try {
        const response = await fetch('https://localhost:7194/api/Auth/me', {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) return null;

        const data = await response.json();
        const welcomeMessage = document.getElementById('welcomeMessage');
        if (welcomeMessage && data.firstName) {
            welcomeMessage.textContent = `Bem-vindo, ${data.firstName}!`;
        }
        return data;
    } catch (error) {
        console.error('Não foi possível carregar o nome do utilizador:', error);
        return null;
    }
}
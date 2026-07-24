document.addEventListener('DOMContentLoaded', () => {
    const btnSubmit = document.getElementById('btnSubmit');

    if (btnSubmit) {
        btnSubmit.addEventListener('click', async (e) => {
            // Garante que NENHUM comportamento padrão de formulário/navegador acontece
            e.preventDefault();
            e.stopPropagation();

            await fazerLogin();
        });
    }
});

async function fazerLogin() {
    const emailInput = document.getElementById('email');
    const passwordInput = document.getElementById('password');
    const errorMessage = document.getElementById('errorMessage');

    if (!emailInput || !passwordInput) return;

    const email = emailInput.value.trim();
    const password = passwordInput.value;

    if (errorMessage) {
        errorMessage.style.display = 'none';
        errorMessage.textContent = '';
    }

    if (!email || !password) {
        exibirErro('Por favor, preencha todos os campos.');
        return;
    }

    try {
        const response = await fetch('/api/Auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });

        const data = await response.json();

        if (response.ok) {
            if (data.token) {
                localStorage.setItem('userToken', data.token);
            }
            // Redirecionamento forçado para a porta 7085
            window.location.href = 'https://localhost:7085/dashboard.html';
        } else {
            exibirErro(data.message || 'E-mail ou palavra-passe incorretos.');
        }
    } catch (error) {
        console.error('Erro na requisição:', error);
        exibirErro('Erro ao conectar ao servidor.');
    }
}

function exibirErro(mensagem) {
    const errorMessage = document.getElementById('errorMessage');
    if (errorMessage) {
        errorMessage.textContent = mensagem;
        errorMessage.style.display = 'block';
    } else {
        alert(mensagem);
    }
}
document.addEventListener('DOMContentLoaded', () => {
    // Gestão do Token na URL
    const urlParams = new URLSearchParams(window.location.search);
    const tokenFromUrl = urlParams.get('token');

    if (tokenFromUrl) {
        localStorage.setItem('jwtToken', tokenFromUrl);
        window.history.replaceState({}, document.title, window.location.pathname);
    }

    // Inicialização da página
    carregarDadosDashboard();
    carregarNomeUtilizador();
});

function terminarSessao() {
    localStorage.removeItem('jwtToken');
    window.location.href = 'https://localhost:7194/login.html';
}

function abrirModalNovaOrdem() {
    document.getElementById('modalNovaOrdem').style.display = 'flex';
}

function fecharModalNovaOrdem() {
    document.getElementById('modalNovaOrdem').style.display = 'none';
}

async function carregarDadosDashboard() {
    try {
        const token = localStorage.getItem('jwtToken');
        const response = await fetch('https://localhost:7085/api/OrdensReparacao', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`
            },
            credentials: 'include'
        });

        if (!response.ok) {
            console.warn("Aviso: A resposta da API não foi 200 OK.");
            return;
        }

        const ordens = await response.json();
        const tbody = document.getElementById('tabelaOrdens');
        tbody.innerHTML = '';

        let total = ordens.length;
        let cur = 0;
        let conc = 0;

        ordens.forEach(ordem => {
            let id = ordem.id || ordem.Id || '';
            let clienteId = ordem.clienteId || ordem.ClienteId || 'N/A';
            let veiculoId = ordem.veiculoId || ordem.VeiculoId || 'N/A';
            let descricao = ordem.descricaoProblema || ordem.DescricaoProblema || 'Sem descrição';
            let valor = ordem.valorTotal !== undefined ? ordem.valorTotal : (ordem.ValorTotal !== undefined ? ordem.ValorTotal : 0);
            let estado = ordem.estado || ordem.Estado || 'Em Curso';

            let estadoClasse = 'pendente';
            if (estado === 'Em Curso' || estado === 'EmCurso') {
                cur++;
                estado = 'Em Curso';
                estadoClasse = 'curso';
            } else if (estado === 'Concluída' || estado === 'Concluida') {
                conc++;
                estado = 'Concluída';
                estadoClasse = 'concluida';
            }

            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><strong>#${id}</strong></td>
                <td>${clienteId}</td>
                <td>${veiculoId}</td>
                <td>${descricao}</td>
                <td><strong>${Number(valor).toFixed(2)} €</strong></td>
                <td><span class="badge ${estadoClasse}"><i class="fa-solid fa-circle" style="font-size: 6px;"></i> ${estado}</span></td>
                <td><button class="btn-action" onclick="alert('Detalhes da Ordem #${id}')"><i class="fa-solid fa-eye"></i> Ver</button></td>
            `;
            tbody.appendChild(tr);
        });

        document.getElementById('totalOrdens').textContent = total;
        document.getElementById('emCurso').textContent = cur;
        document.getElementById('concluidas').textContent = conc;

    } catch (error) {
        console.error("Erro de ligação:", error);
    }
}

async function criarOrdemReparacao(e) {
    e.preventDefault();

    const token = localStorage.getItem('jwtToken');
    const pecaIdVal = document.getElementById('pecaIdInput').value;
    const qtdVal = parseInt(document.getElementById('quantidadePecaInput').value) || 0;

    let pecasArray = [];
    if (pecaIdVal && qtdVal > 0) {
        pecasArray.push({
            pecaId: pecaIdVal,
            quantidade: qtdVal
        });
    }

    const novaOrdem = {
        clienteId: document.getElementById('clienteIdInput').value,
        veiculoId: parseInt(document.getElementById('veiculoIdInput').value),
        custoMaoDeObra: parseFloat(document.getElementById('custoMaoDeObraInput').value),
        descricaoProblema: document.getElementById('descricaoInput').value,
        pecas: pecasArray
    };

    try {
        const response = await fetch('https://localhost:7085/api/OrdensReparacao/repair-order', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(novaOrdem)
        });

        if (response.ok) {
            alert('Ordem de reparação criada com sucesso!');
            fecharModalNovaOrdem();
            carregarDadosDashboard();
        } else {
            const errData = await response.json().catch(() => ({}));
            alert('Erro ao criar ordem: ' + (errData.mensagem || errData.message || 'Verifique o stock ou os dados inseridos.'));
        }
    } catch (err) {
        console.error(err);
        alert('Erro de comunicação ao submeter a ordem.');
    }
}

async function carregarNomeUtilizador() {
    try {
        const token = localStorage.getItem('jwtToken');
        const response = await fetch('https://localhost:7194/api/Auth/me', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`
            },
            credentials: 'include'
        });

        if (!response.ok) return;

        const data = await response.json();

        if (data.firstName) {
            document.getElementById('welcomeMessage').textContent = `Bem-vindo, ${data.firstName}!`;
        }
    } catch (error) {
        console.error('Não foi possível carregar o nome do utilizador:', error);
    }
}
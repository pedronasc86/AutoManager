const urlParams = new URLSearchParams(window.location.search);
const tokenFromUrl = urlParams.get('token');

if (tokenFromUrl) {
    window.history.replaceState({}, document.title, window.location.pathname);
}

function terminarSessao() {
    window.location.href = 'https://localhost:7194/login.html';
}

function abrirModalNovaOrdem() {
    document.getElementById('modalNovaOrdem').style.display = 'flex';
}

function fecharModalNovaOrdem() {
    document.getElementById('modalNovaOrdem').style.display = 'none';
}

const titulosSecoes = {
    dashboard: 'Painel Geral da Oficina',
    ordens: 'Ordens de Reparação',
    pecas: 'Catálogo de Peças',
    veiculos: 'Veículos',
    clientes: 'Clientes',
    admins: 'Administradores'
};

let secaoAtual = 'dashboard';

function mostrarSecao(secao) {
    secaoAtual = secao;

    const mostrarDashboard = secao === 'dashboard';
    const mostrarOrdens = secao === 'dashboard' || secao === 'ordens';

    document.getElementById('btnNovaOrdem')
        .classList.toggle('view-hidden', secao !== 'ordens');

    document.getElementById('cabecalhoAcoesOrdens')
        .classList.toggle('view-hidden', secao !== 'ordens');

    document.getElementById('secaoPecas')
        .classList.toggle('view-hidden', secao !== 'pecas');

    document.getElementById('tituloPagina').textContent = titulosSecoes[secao];

    document.getElementById('dashboardCards')
        .classList.toggle('view-hidden', !mostrarDashboard);

    document.getElementById('secaoOrdens')
        .classList.toggle('view-hidden', !mostrarOrdens);

    document.getElementById('secaoVeiculos')
        .classList.toggle('view-hidden', secao !== 'veiculos');

    document.getElementById('secaoClientes')
        .classList.toggle('view-hidden', secao !== 'clientes');

    document.getElementById('secaoAdmins')
        .classList.toggle('view-hidden', secao !== 'admins');

    document.querySelectorAll('.menu li').forEach(item => {
        item.classList.toggle('active', item.dataset.section === secao);
    });

    if (secao === 'dashboard' || secao === 'ordens') {
        carregarDadosDashboard();
    } else if (secao === 'veiculos') {
        carregarVeiculos();
    } else if (secao === 'pecas') {
        carregarPecas();
    } else if (secao === 'clientes') {
        carregarClientes();
    } else if (secao === 'admins') {
        carregarAdmins();
    }
}

async function carregarPecas() {
    const tabela = document.getElementById('tabelaPecas');
    tabela.innerHTML = '';

    try {
        const response = await fetch(
            'https://localhost:7085/api/Catalogo/pecas',
            {
                method: 'GET',
                credentials: 'include'
            }
        );

        if (!response.ok) {
            throw new Error('Não foi possível carregar as peças.');
        }

        const pecas = await response.json();

        if (pecas.length === 0) {
            mostrarTabelaVazia(
                'tabelaPecas',
                7,
                'Não existem peças registadas.'
            );
            return;
        }

        tabela.innerHTML = pecas.map(peca => `
            <tr>
                <td>${escaparHtml(peca.referenciaPeca)}</td>
                <td>${escaparHtml(peca.nome)}</td>
                <td>${escaparHtml(peca.categoria)}</td>
                <td>${escaparHtml(peca.compatibilidade)}</td>
                <td><strong>${Number(peca.precoUnitario).toFixed(2)} €</strong></td>
                <td>${peca.stockDisponivel}</td>
                <td>
                    <span class="badge ${peca.ativo ? 'concluida' : 'pendente'}">
                        ${peca.ativo ? 'Ativa' : 'Inativa'}
                    </span>
                </td>
            </tr>
        `).join('');
    } catch (error) {
        console.error(error);

        mostrarTabelaVazia(
            'tabelaPecas',
            7,
            'Erro ao carregar as peças.'
        );
    }
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
    document.getElementById(tabelaId).innerHTML = `
        <tr>
            <td colspan="${numeroColunas}" class="empty-state">
                ${escaparHtml(mensagem)}
            </td>
        </tr>`;
}

async function carregarVeiculos() {
    const tabela = document.getElementById('tabelaVeiculos');
    tabela.innerHTML = '';

    try {
        const response = await fetch('https://localhost:7085/api/Veiculos', {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Não foi possível carregar os veículos.');
        }

        const veiculos = await response.json();

        if (veiculos.length === 0) {
            mostrarTabelaVazia(
                'tabelaVeiculos',
                6,
                'Não existem veículos registados.'
            );
            return;
        }

        tabela.innerHTML = veiculos.map(veiculo => `
            <tr>
                <td><strong>#${veiculo.id}</strong></td>
                <td>${escaparHtml(veiculo.matricula)}</td>
                <td>${escaparHtml(veiculo.marca)}</td>
                <td>${escaparHtml(veiculo.modelo)}</td>
                <td>${veiculo.ano}</td>
                <td>${escaparHtml(veiculo.clienteId)}</td>
            </tr>
        `).join('');
    } catch (error) {
        console.error(error);
        mostrarTabelaVazia(
            'tabelaVeiculos',
            6,
            'Erro ao carregar os veículos.'
        );
    }
}

async function carregarClientes() {
    const tabela = document.getElementById('tabelaClientes');
    tabela.innerHTML = '';

    try {
        const response = await fetch('https://localhost:7194/api/Auth/users', {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Não foi possível carregar os clientes.');
        }

        const clientes = await response.json();

        if (clientes.length === 0) {
            mostrarTabelaVazia(
                'tabelaClientes',
                4,
                'Não existem clientes registados.'
            );
            return;
        }

        tabela.innerHTML = clientes.map(cliente => `
            <tr>
                <td class="user-id">${escaparHtml(cliente.id)}</td>
                <td>${escaparHtml(cliente.firstName)}</td>
                <td>${escaparHtml(cliente.email)}</td>
                <td>${escaparHtml(cliente.role || 'Sem perfil')}</td>
            </tr>
        `).join('');
    } catch (error) {
        console.error(error);
        mostrarTabelaVazia(
            'tabelaClientes',
            4,
            'Erro ao carregar os clientes.'
        );
    }
}

let adminsCarregados = [];

async function carregarAdmins() {
    const tabela = document.getElementById('tabelaAdmins');
    tabela.innerHTML = '';

    try {
        const response = await fetch(
            'https://localhost:7194/api/Auth/admins',
            {
                method: 'GET',
                credentials: 'include'
            }
        );

        if (!response.ok) {
            throw new Error('Não foi possível carregar os administradores.');
        }

        const admins = await response.json();
        adminsCarregados = admins;

        if (admins.length === 0) {
            mostrarTabelaVazia(
                'tabelaAdmins',
                5,
                'Não existem administradores registados.'
            );
            return;
        }

        tabela.innerHTML = admins.map(admin => `
            <tr>
                <td class="user-id">${escaparHtml(admin.id)}</td>
                <td>${escaparHtml(admin.firstName)}</td>
                <td>${escaparHtml(admin.email)}</td>
                <td>${escaparHtml(admin.role)}</td>
                <td>
                    <button class="btn-action btn-edit"
                            onclick="abrirModalEditarAdmin('${admin.id}')">
                        <i class="fa-solid fa-pen"></i> Editar
                    </button>

                    <button class="btn-action btn-delete"
                            onclick="eliminarAdmin('${admin.id}')">
                        <i class="fa-solid fa-trash"></i> Eliminar
                    </button>
                </td>
            </tr>
        `).join('');
    } catch (error) {
        console.error(error);

        mostrarTabelaVazia(
            'tabelaAdmins',
            5,
            'Erro ao carregar os administradores.'
        );
    }
}

let paginaAtual = 1;
let totalPaginas = 1;
let filtroVeiculoId = null;

function mudarPagina(direcao) {
    const novaPagina = paginaAtual + direcao;

    if (novaPagina < 1 || novaPagina > totalPaginas) {
        return;
    }

    paginaAtual = novaPagina;
    carregarDadosDashboard();
}

function aplicarFiltroVeiculo() {
    const valor = document.getElementById('filtroVeiculoId').value;

    if (!valor || Number(valor) <= 0) {
        alert('Introduz um ID de veículo válido.');
        return;
    }

    filtroVeiculoId = Number(valor);
    paginaAtual = 1;
    carregarDadosDashboard();
}

function limparFiltroVeiculo() {
    document.getElementById('filtroVeiculoId').value = '';
    filtroVeiculoId = null;
    paginaAtual = 1;
    carregarDadosDashboard();
}

async function carregarDadosDashboard() {
    try {
        const parametros = new URLSearchParams({
            pagina: paginaAtual,
            tamanhoPagina: 5
        });

        if (filtroVeiculoId) {
            parametros.append('veiculoId', filtroVeiculoId);
        }

        const response = await fetch(
            `https://localhost:7085/api/OrdensReparacao?${parametros.toString()}`,
            {
                method: 'GET',
                credentials: 'include'
            }
        );

        if (!response.ok) {
            console.warn('Não foi possível carregar as ordens.');
            return;
        }

        const dados = await response.json();
        const tbody = document.getElementById('tabelaOrdens');

        tbody.innerHTML = '';

        if (dados.itens.length === 0) {
            tbody.innerHTML = `
            <tr>
                <td colspan="${secaoAtual === 'ordens' ? 7 : 6}" style="text-align: center; padding: 25px;">
                    Não existem ordens para este veículo.
                </td>
            </tr>
        `;
        }

        dados.itens.forEach(ordem => {
            let estado = ordem.estado || 'Em Curso';
            let estadoClasse = estado === 'Em Curso' ? 'curso' : 'concluida';

            const tr = document.createElement('tr');

            const acoesOrdem = secaoAtual === 'ordens' ? `
            <td>
                <button class="btn-action" onclick="verDetalhesOrdem(${ordem.id})">
                    <i class="fa-solid fa-eye"></i> Ver
                </button>

                <button class="btn-action" onclick="abrirEdicaoOrdem(${ordem.id})">
                    <i class="fa-solid fa-pen"></i> Editar
                </button>
            </td>
        ` : '';

            tr.innerHTML = `
            <td><strong>#${ordem.id}</strong></td>
            <td>${ordem.clienteId}</td>
            <td>${ordem.veiculoId}</td>
            <td>${ordem.descricaoProblema}</td>
            <td><strong>${Number(ordem.valorTotal).toFixed(2)} €</strong></td>
            <td>
                <span class="badge ${estadoClasse}">
                    <i class="fa-solid fa-circle" style="font-size: 6px;"></i>
                    ${estado}
                </span>
            </td>
            ${acoesOrdem}
        `;

            tbody.appendChild(tr);
        });

        document.getElementById('totalOrdens').textContent = dados.totalOrdens;
        document.getElementById('emCurso').textContent = dados.totalEmCurso;
        document.getElementById('concluidas').textContent = dados.totalConcluidas;

        paginaAtual = dados.paginaAtual;
        totalPaginas = dados.totalPaginas;

        document.getElementById('infoPagina').textContent =
            `Página ${paginaAtual} de ${totalPaginas} (${dados.totalItens} ordem(ns))`;

        document.getElementById('btnPaginaAnterior').disabled = paginaAtual === 1;
        document.getElementById('btnPaginaSeguinte').disabled =
            paginaAtual === totalPaginas;

    } catch (error) {
        console.error('Erro de ligação:', error);
    }
}

async function criarOrdemReparacao(e) {
    e.preventDefault();

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
        // Compatível com o endpoint /repair-order do RF8
        const response = await fetch('https://localhost:7085/api/OrdensReparacao/repair-order', {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(novaOrdem)
        });

        if (response.ok) {
            const mensagem = document.getElementById('mensagemOrdem');

            mensagem.textContent = 'Ordem de reparação criada com sucesso!';
            mensagem.className = 'mensagem-ordem sucesso';

            setTimeout(() => {
                fecharModalNovaOrdem();

                document.getElementById('formNovaOrdem').reset();

                mensagem.textContent = '';
                mensagem.className = 'mensagem-ordem';

                carregarDadosDashboard();
            }, 3000);
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
        const response = await fetch('https://localhost:7194/api/Auth/me', {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) return;

        const data = await response.json();

        if (data.firstName) {
            document.getElementById('welcomeMessage').textContent =
                `Bem-vindo, ${data.firstName}!`;
        }

        if (data.role === 'Admin') {
            const botao = document.getElementById('btnCriarUtilizador');

            if (botao) {
                botao.style.display = 'inline-block';

                botao.addEventListener('click', () => {
                    window.location.href = 'https://localhost:7194/CreateUser.html';
                });
            }
        }
    } catch (error) {
        console.error('Não foi possível carregar o nome do utilizador:', error);
    }
}

let ordemDetalheAtual = null;

function formatarMoeda(valor) {
    return `${Number(valor || 0).toFixed(2)} €`;
}

function formatarData(data) {
    return data
        ? new Date(data).toLocaleString('pt-PT')
        : 'Ainda não concluída';
}

function fecharModalDetalhes() {
    document.getElementById('modalDetalhesOrdem').style.display = 'none';
}

function preencherDetalhesOrdem(ordem) {
    document.getElementById('tituloDetalheOrdem').textContent =
        `Detalhes da Ordem #${ordem.id}`;

    document.getElementById('detalheCliente').textContent = ordem.clienteId;
    document.getElementById('detalheVeiculo').textContent = `#${ordem.veiculoId}`;
    document.getElementById('detalheDataEntrada').textContent =
        formatarData(ordem.dataEntrada);
    document.getElementById('detalheDataConclusao').textContent =
        formatarData(ordem.dataConclusao);
    document.getElementById('detalheMaoDeObra').textContent =
        formatarMoeda(ordem.custoMaoDeObra);
    document.getElementById('detalheCustoPecas').textContent =
        formatarMoeda(ordem.custoPecas);
    document.getElementById('detalheTotal').textContent =
        formatarMoeda(ordem.valorTotal);
    document.getElementById('detalheDescricao').textContent =
        ordem.descricaoProblema;

    document.getElementById('estadoOrdem').value = ordem.estado;

    const listaPecas = document.getElementById('listaPecasDetalhe');

    if (!ordem.pecas || ordem.pecas.length === 0) {
        listaPecas.innerHTML = '<p>Sem peças registadas nesta ordem.</p>';
        return;
    }

    listaPecas.innerHTML = ordem.pecas.map(peca => `
<div class="peca-detalhe">
    <span>${peca.pecaId}</span>
    <span>${peca.quantidade} un. × ${formatarMoeda(peca.precoUnitario)}</span>
    <strong>${formatarMoeda(peca.subtotal)}</strong>
</div>
`).join('');
}

async function verDetalhesOrdem(id) {
    try {
        const response = await fetch(
            `https://localhost:7085/api/OrdensReparacao/${id}`,
            {
                method: 'GET',
                credentials: 'include'
            }
        );

        if (!response.ok) {
            alert('Não foi possível obter os detalhes da ordem.');
            return;
        }

        ordemDetalheAtual = await response.json();
        preencherDetalhesOrdem(ordemDetalheAtual);

        document.getElementById('modalDetalhesOrdem').style.display = 'flex';
    } catch (error) {
        console.error(error);
        alert('Erro de comunicação ao carregar os detalhes.');
    }
}

async function alterarEstadoOrdem() {
    if (!ordemDetalheAtual) return;

    const estado = document.getElementById('estadoOrdem').value;
    const mensagem = document.getElementById('mensagemDetalheOrdem');

    try {
        const response = await fetch(
            `https://localhost:7085/api/OrdensReparacao/${ordemDetalheAtual.id}`,
            {
                method: 'PUT',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ estado })
            }
        );

        const dados = await response.json().catch(() => ({}));

        if (!response.ok) {
            mensagem.textContent =
                dados.mensagem || 'Não foi possível alterar o estado.';
            mensagem.className = 'mensagem-ordem erro';
            return;
        }

        ordemDetalheAtual = { ...ordemDetalheAtual, ...dados };
        preencherDetalhesOrdem(ordemDetalheAtual);

        mensagem.textContent = 'Estado atualizado com sucesso.';
        mensagem.className = 'mensagem-ordem sucesso';

        carregarDadosDashboard();
    } catch (error) {
        console.error(error);
        mensagem.textContent = 'Erro de comunicação ao atualizar o estado.';
        mensagem.className = 'mensagem-ordem erro';
    }
}

let ordemEmEdicao = null;

function fecharModalEditarOrdem() {
    document.getElementById('modalEditarOrdem').style.display = 'none';
}

async function abrirEdicaoOrdem(id) {
    try {
        const response = await fetch(
            `https://localhost:7085/api/OrdensReparacao/${id}`,
            {
                method: 'GET',
                credentials: 'include'
            }
        );

        if (!response.ok) {
            throw new Error('Não foi possível obter a ordem.');
        }

        ordemEmEdicao = await response.json();

        document.getElementById('tituloEditarOrdem').textContent =
            `Editar Ordem #${ordemEmEdicao.id}`;

        document.getElementById('editarDescricaoOrdem').value =
            ordemEmEdicao.descricaoProblema || '';

        document.getElementById('editarMaoDeObraOrdem').value =
            ordemEmEdicao.custoMaoDeObra ?? 0;

        document.getElementById('editarEstadoOrdem').value =
            ordemEmEdicao.estado || 'Em Curso';

        document.getElementById('mensagemEditarOrdem').textContent = '';
        document.getElementById('mensagemEditarOrdem').className =
            'mensagem-ordem';

        document.getElementById('modalEditarOrdem').style.display = 'flex';
    } catch (error) {
        console.error(error);
        alert('Não foi possível abrir a edição da ordem.');
    }
}

async function guardarEdicaoOrdem(event) {
    event.preventDefault();

    if (!ordemEmEdicao) return;

    const mensagem = document.getElementById('mensagemEditarOrdem');

    const dadosAtualizados = {
        descricaoProblema: document.getElementById('editarDescricaoOrdem').value,
        custoMaoDeObra: Number(
            document.getElementById('editarMaoDeObraOrdem').value
        ),
        estado: document.getElementById('editarEstadoOrdem').value
    };

    try {
        const response = await fetch(
            `https://localhost:7085/api/OrdensReparacao/${ordemEmEdicao.id}`,
            {
                method: 'PUT',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(dadosAtualizados)
            }
        );

        if (!response.ok) {
            const erro = await response.json().catch(() => ({}));

            mensagem.textContent =
                erro.mensagem || erro.message || 'Não foi possível guardar as alterações.';
            mensagem.className = 'mensagem-ordem erro';
            return;
        }

        mensagem.textContent = 'Ordem atualizada com sucesso!';
        mensagem.className = 'mensagem-ordem sucesso';

        carregarDadosDashboard();

        setTimeout(() => {
            fecharModalEditarOrdem();
            mensagem.textContent = '';
            mensagem.className = 'mensagem-ordem';
        }, 1500);
    } catch (error) {
        console.error(error);
        mensagem.textContent = 'Erro de comunicação ao atualizar a ordem.';
        mensagem.className = 'mensagem-ordem erro';
    }
}

function fecharModalAdmin() {
    document.getElementById('modalAdmin').style.display = 'none';
}

function limparMensagemAdmin() {
    const mensagem = document.getElementById('mensagemAdmin');
    mensagem.textContent = '';
    mensagem.className = 'mensagem-ordem';
}

function abrirModalNovoAdmin() {
    document.getElementById('formAdmin').reset();
    document.getElementById('adminEdicaoId').value = '';

    document.getElementById('tituloModalAdmin').textContent =
        'Novo Administrador';

    document.getElementById('labelPasswordAdmin').textContent =
        'Password';

    document.getElementById('ajudaPasswordAdmin').textContent =
        'Obrigatória ao criar um administrador.';

    limparMensagemAdmin();

    document.getElementById('modalAdmin').style.display = 'flex';
}

function abrirModalEditarAdmin(id) {
    const admin = adminsCarregados.find(item => item.id === id);

    if (!admin) {
        alert('Não foi possível encontrar o administrador.');
        return;
    }

    document.getElementById('adminEdicaoId').value = admin.id;
    document.getElementById('adminPrimeiroNome').value = admin.firstName;
    document.getElementById('adminEmail').value = admin.email;
    document.getElementById('adminPassword').value = '';

    document.getElementById('tituloModalAdmin').textContent =
        'Editar Administrador';

    document.getElementById('labelPasswordAdmin').textContent =
        'Nova password';

    document.getElementById('ajudaPasswordAdmin').textContent =
        'Opcional. Deixa vazia para manter a password atual.';

    limparMensagemAdmin();

    document.getElementById('modalAdmin').style.display = 'flex';
}

async function guardarAdmin(event) {
    event.preventDefault();

    const id = document.getElementById('adminEdicaoId').value;
    const primeiroNome =
        document.getElementById('adminPrimeiroNome').value.trim();
    const email = document.getElementById('adminEmail').value.trim();
    const password = document.getElementById('adminPassword').value;

    const mensagem = document.getElementById('mensagemAdmin');

    if (!id && !password) {
        mensagem.textContent =
            'A password é obrigatória ao criar um administrador.';
        mensagem.className = 'mensagem-ordem erro';
        return;
    }

    const dadosAdmin = {
        firstName: primeiroNome,
        email: email
    };

    if (password) {
        dadosAdmin.password = password;
    }

    const url = id
        ? `https://localhost:7194/api/Auth/admins/${id}`
        : 'https://localhost:7194/api/Auth/admins';

    const metodo = id ? 'PUT' : 'POST';

    try {
        const response = await fetch(url, {
            method: metodo,
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dadosAdmin)
        });

        if (!response.ok) {
            const erro = await response.json().catch(() => ({}));

            mensagem.textContent =
                erro.message || 'Não foi possível guardar o administrador.';
            mensagem.className = 'mensagem-ordem erro';
            return;
        }

        mensagem.textContent = id
            ? 'Administrador atualizado com sucesso!'
            : 'Administrador criado com sucesso!';

        mensagem.className = 'mensagem-ordem sucesso';

        await carregarAdmins();

        setTimeout(() => {
            fecharModalAdmin();
            limparMensagemAdmin();
        }, 1500);
    } catch (error) {
        console.error(error);

        mensagem.textContent =
            'Erro de comunicação ao guardar o administrador.';
        mensagem.className = 'mensagem-ordem erro';
    }
}

async function eliminarAdmin(id) {
    const admin = adminsCarregados.find(item => item.id === id);

    const confirmar = confirm(
        `Queres eliminar o administrador "${admin?.firstName || ''}"?`
    );

    if (!confirmar) {
        return;
    }

    try {
        const response = await fetch(
            `https://localhost:7194/api/Auth/admins/${id}`,
            {
                method: 'DELETE',
                credentials: 'include'
            }
        );

        if (!response.ok) {
            const erro = await response.json().catch(() => ({}));

            alert(
                erro.message ||
                'Não foi possível eliminar o administrador.'
            );

            return;
        }

        await carregarAdmins();
    } catch (error) {
        console.error(error);
        alert('Erro de comunicação ao eliminar o administrador.');
    }
}

carregarDadosDashboard();
carregarNomeUtilizador();
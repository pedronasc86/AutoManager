const urlParams = new URLSearchParams(window.location.search);
const tokenFromUrl = urlParams.get('token');

if (tokenFromUrl) {
    localStorage.setItem('token', tokenFromUrl);
    window.history.replaceState({}, document.title, window.location.pathname);
}

const tokenCookie = obterCookie('jwtToken');
if (tokenCookie && !localStorage.getItem('token')) {
    localStorage.setItem('token', tokenCookie);
}

// Termina a sessão e redireciona para o login
function terminarSessao() {
    localStorage.removeItem('token');
    window.location.href = 'https://localhost:7194/login.html';
}

// Modal de Nova Ordem
function abrirModalNovaOrdem() {
    document.getElementById('modalNovaOrdem').style.display = 'flex';
}

function fecharModalNovaOrdem() {
    document.getElementById('modalNovaOrdem').style.display = 'none';
}

function obterCookie(nome) {
    const valor = `; ${document.cookie}`;
    const partes = valor.split(`; ${nome}=`);
    if (partes.length === 2) return partes.pop().split(';').shift();
}

// Uso:
const token = obterCookie('jwtToken');

// Mapeamento dos títulos das secções
const titulosSecoes = {
    dashboard: 'Painel Geral da Oficina',
    pedidos: 'Gestão de Pedidos',
    ordens: 'Ordens de Reparação',
    pecas: 'Catálogo de Peças',
    veiculos: 'Veículos',
    clientes: 'Clientes',
    admins: 'Administradores'
};

let secaoAtual = 'dashboard';

// Alterna a exibição das vistas/secções
function mostrarSecao(secao) {
    secaoAtual = secao;

    //const mostrarDashboard = secao === 'dashboard';
    const mostrarDashboard = secao === 'dashboard' || secao === 'ordens';
    const mostrarOrdens = secao === 'dashboard' || secao === 'ordens';

    const btnNovaOrdemEl = document.getElementById('btnNovaOrdem');
    if (btnNovaOrdemEl) {
        btnNovaOrdemEl.classList.toggle('view-hidden', secao !== 'ordens');
    }

    const cabecalhoAcoesOrdensEl = document.getElementById('cabecalhoAcoesOrdens');
    if (cabecalhoAcoesOrdensEl) {
        cabecalhoAcoesOrdensEl.classList.toggle('view-hidden', secao !== 'ordens');
    }

    const cabecalhoAcoesClientes = document.getElementById('cabecalhoAcoesClientes');
    if (cabecalhoAcoesClientes) {
        cabecalhoAcoesClientes.classList.toggle('view-hidden', secao !== 'clientes');
    }

    const secaoPecasEl = document.getElementById('secaoPecas');
    if (secaoPecasEl) {
        secaoPecasEl.classList.toggle('view-hidden', secao !== 'pecas');
    }

    // Os cartões das peças só aparecem ao abrir a secção "Peças".
    const pecasCardsEl = document.getElementById('pecasCards');

    if (pecasCardsEl) {
        pecasCardsEl.classList.toggle('view-hidden', secao !== 'pecas');
    }

    const tituloPaginaEl = document.getElementById('tituloPagina');
    if (tituloPaginaEl) {
        tituloPaginaEl.textContent = titulosSecoes[secao] || 'Oficina';
    }

    const dashboardCardsEl = document.getElementById('dashboardCards');
    if (dashboardCardsEl) {
        dashboardCardsEl.classList.toggle('view-hidden', !mostrarDashboard);
    }

    const secaoOrdensEl = document.getElementById('secaoOrdens');
    if (secaoOrdensEl) {
        secaoOrdensEl.classList.toggle('view-hidden', !mostrarOrdens);
    }

    const secaoVeiculosEl = document.getElementById('secaoVeiculos');
    if (secaoVeiculosEl) {
        secaoVeiculosEl.classList.toggle('view-hidden', secao !== 'veiculos');
    }

    const secaoClientesEl = document.getElementById('secaoClientes');
    if (secaoClientesEl) {
        secaoClientesEl.classList.toggle('view-hidden', secao !== 'clientes');
    }

    const secaoAdminsEl = document.getElementById('secaoAdmins');
    if (secaoAdminsEl) {
        secaoAdminsEl.classList.toggle('view-hidden', secao !== 'admins');
    }

    const secaoPedidosEl = document.getElementById('secaoPedidos');
    if (secaoPedidosEl) {
        secaoPedidosEl.classList.toggle('view-hidden', secao !== 'pedidos');
    }

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
    } else if (secao === 'pedidos') {
        carregarPedidos();
    }
}

// Escapa carateres HTML para prevenir XSS
function escaparHtml(valor) {
    return String(valor ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

// Exibe mensagem de tabela vazia
function mostrarTabelaVazia(tabelaId, numeroColunas, mensagem) {
    const tabelaEl = document.getElementById(tabelaId);
    if (tabelaEl) {
        tabelaEl.innerHTML = `
            <tr>
                <td colspan="${numeroColunas}" class="empty-state" style="text-align: center; padding: 25px;">
                    ${escaparHtml(mensagem)}
                </td>
            </tr>`;
    }
}

// Carregar pedidos de reparação para a tabela
async function carregarPedidos() {
    const tabela = document.getElementById('tabelaGestaoPedidos');
    if (!tabela) return;
    tabela.innerHTML = '';

    try {
        const response = await fetch('https://localhost:7085/api/pedidos/pendentes', {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error(`Erro HTTP: ${response.status}`);
        }

        const data = await response.json();
        const pedidos = Array.isArray(data) ? data : (data.itens || data.pedidos || []);

        window.listaPedidosCache = pedidos;

        if (pedidos.length === 0) {
            mostrarTabelaVazia(
                'tabelaGestaoPedidos',
                7,
                'Não existem pedidos de reparação pendentes.'
            );
            return;
        }

        tabela.innerHTML = pedidos.map(pedido => `
            <tr>
                <td>${formatarData(pedido.dataCriacao || pedido.dataEntrada || pedido.data)}</td>
                <td>${escaparHtml(pedido.clienteId)}</td>
                <td>${escaparHtml(pedido.veiculoId)}</td>
                <td>${escaparHtml(pedido.problemaReportado || pedido.descricaoProblema || pedido.descricao)}</td>
                <td>
                    <span class="badge curso">
                        ${escaparHtml(pedido.estado || 'Pendente')}
                    </span>
                </td>
                <td>${escaparHtml(pedido.observacoes || '-')}</td>
                <td>
                    <button class="btn-action" onclick="verDetalhesPedido(${pedido.id})">
                        <i class="fa-solid fa-eye"></i> Ver
                    </button>
                </td>
            </tr>
        `).join('');
    } catch (error) {
        console.error(error);
        mostrarTabelaVazia(
            'tabelaGestaoPedidos',
            7,
            'Erro ao carregar os pedidos.'
        );
    }
}

let pedidoAtualEmAnalise = null;

function verDetalhesPedido(id) {
    const pedido = window.listaPedidosCache?.find(p => p.id === id);

    if (!pedido) {
        alert('Não foi possível encontrar os dados do pedido.');
        return;
    }

    pedidoAtualEmAnalise = pedido;

    document.getElementById('tituloModalPedido').textContent = `Analisar Pedido #${pedido.id}`;
    document.getElementById('pedidoClienteId').textContent = pedido.clienteId;
    document.getElementById('pedidoVeiculoId').textContent = pedido.veiculoId;
    document.getElementById('pedidoProblema').textContent = pedido.problemaReportado || pedido.descricaoProblema || pedido.descricao || '-';

    document.getElementById('pedidoIdEdicao').value = pedido.id;
    document.getElementById('pedidoNovoEstado').value = pedido.estado || 'Aprovado';
    document.getElementById('pedidoObservacoes').value = pedido.observacoes || '';

    document.getElementById('mensagemAnalisePedido').textContent = '';
    document.getElementById('mensagemAnalisePedido').className = 'mensagem-ordem';

    document.getElementById('modalDetalhesPedido').style.display = 'flex';
}

function fecharModalDetalhesPedido() {
    document.getElementById('modalDetalhesPedido').style.display = 'none';
}

async function guardarAnalisePedido(event) {
    event.preventDefault();

    if (!pedidoAtualEmAnalise) return;

    const id = pedidoAtualEmAnalise.id;
    const estado = document.getElementById('pedidoNovoEstado').value;
    const observacoes = document.getElementById('pedidoObservacoes').value;
    const mensagem = document.getElementById('mensagemAnalisePedido');

    try {
        const response = await fetch(`https://localhost:7085/api/pedidos/${id}/analisar`, {
            method: 'PATCH',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                estado: estado,
                observacoes: observacoes
            })
        });

        if (!response.ok) {
            const erro = await response.json().catch(() => ({}));
            mensagem.textContent = erro.mensagem || 'Erro ao atualizar o pedido.';
            mensagem.className = 'mensagem-ordem erro';
            return;
        }

        mensagem.textContent = 'Pedido analisado com sucesso!';
        mensagem.className = 'mensagem-ordem sucesso';

        setTimeout(() => {
            fecharModalDetalhesPedido();
            carregarPedidos();
        }, 1200);

    } catch (error) {
        console.error(error);
        mensagem.textContent = 'Erro de comunicação com o servidor.';
        mensagem.className = 'mensagem-ordem erro';
    }
}

// Atualiza os três cartões de resumo do inventário.
function atualizarResumoPecas(pecas) {
    const total = pecas.length;

    // Uma peça está em stock se tiver pelo menos uma unidade disponível.
    const emStock = pecas.filter(peca => {
        const stock = Number(peca.stockDisponivel ?? peca.StockDisponivel ?? 0);
        return stock > 0;
    }).length;

    const foraStock = total - emStock;

    document.getElementById('totalPecas').textContent = total;
    document.getElementById('pecasEmStock').textContent = emStock;
    document.getElementById('pecasForaStock').textContent = foraStock;
}

async function carregarPecas() {
    const tabela = document.getElementById('tabelaPecas');
    if (!tabela) return;
    tabela.innerHTML = '';

    try {
        const inputPesquisa = document.getElementById('filtroPecaPesquisa');
        const selectCategoria = document.getElementById('filtroPecaCategoria');
        const selectEstado = document.getElementById('filtroPecaEstado');

        let url = 'http://localhost:5039/api/pecas/admin/todas';
        let queryParams = [];

        if (inputPesquisa && inputPesquisa.value.trim()) {
            const termo = inputPesquisa.value.trim();
            queryParams.push(`contains(nome, '${termo}') or contains(referenciaPeca, '${termo}')`);
        }

        if (selectCategoria && selectCategoria.value) {
            queryParams.push(`categoria eq '${selectCategoria.value}'`);
        }

        if (selectEstado && selectEstado.value !== '') {
            queryParams.push(`ativo eq ${selectEstado.value}`);
        }

        if (queryParams.length > 0) {
            url += `?$filter=${queryParams.join(' and ')}`;
        }

        const token = localStorage.getItem('token');

        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Authorization': token ? `Bearer ${token}` : '',
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Falha ao buscar as peças.');
        }

        const data = await response.json();
        const pecas = Array.isArray(data) ? data : (data.value || data.itens || []);

        atualizarResumoPecas(pecas);

        // Se a API falhar, os cartões não mostram valores antigos.
        atualizarResumoPecas([]);

        if (pecas.length === 0) {
            mostrarTabelaVazia(
                'tabelaPecas',
                7,
                'Não existem peças registadas com os filtros selecionados.'
            );
            return;
        }

        tabela.innerHTML = pecas.map(peca => {
            const id = peca.id ?? peca.Id;
            const referencia = escaparHtml(peca.referenciaPeca ?? peca.ReferenciaPeca ?? '-');
            const nome = escaparHtml(peca.nome ?? peca.Nome ?? '-');
            const categoria = escaparHtml(peca.categoria ?? peca.Categoria ?? '-');
            const compatibilidade = escaparHtml(peca.compatibilidade ?? peca.Compatibilidade ?? 'Geral');
            const preco = Number(peca.precoUnitario ?? peca.PrecoUnitario ?? 0).toFixed(2);
            const stock = peca.stockDisponivel ?? peca.StockDisponivel ?? 0;
            const ativo = peca.ativo ?? peca.Ativo ?? true;

            return `
                <tr>
                    <td>${referencia}</td>
                    <td>${nome}</td>
                    <td>${categoria}</td>
                    <td>${compatibilidade}</td>
                    <td><strong>${preco} €</strong></td>
                    <td>${stock}</td>
                    <td>
                        <span class="badge ${ativo ? 'concluida' : 'pendente'}">
                            ${ativo ? 'Ativa' : 'Inativa'}
                        </span>
                    </td>
                    <td>
                        <button class="btn-action btn-edit" onclick="abrirModalEditarPeca('${id}')" title="Editar">
                            <i class="fa-solid fa-pen"></i>
                        </button>
                        <button class="btn-action ${ativo ? 'btn-warning' : 'btn-success'}" onclick="alternarInativarPeca('${id}', ${ativo})" title="${ativo ? 'Inativar' : 'Ativar'}">
                            <i class="fa-solid ${ativo ? 'fa-ban' : 'fa-check'}"></i>
                        </button>
                        <button class="btn-action btn-delete" onclick="eliminarPeca('${id}')" title="Eliminar">
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        }).join('');
    } catch (error) {
        console.error(error);
        mostrarTabelaVazia(
            'tabelaPecas',
            8,
            'Erro ao carregar as peças.'
        );
    }
}

function aplicarFiltrosPecas() {
    carregarPecas();
}

function limparFiltrosPecas() {
    if (document.getElementById('filtroPecaPesquisa')) document.getElementById('filtroPecaPesquisa').value = '';
    if (document.getElementById('filtroPecaCategoria')) document.getElementById('filtroPecaCategoria').value = '';
    if (document.getElementById('filtroPecaEstado')) document.getElementById('filtroPecaEstado').value = '';
    carregarPecas();
}

let pecaEmEdicaoId = null;

function abrirModalNovaPeca() {
    pecaEmEdicaoId = null;
    document.getElementById('tituloModalPeca').textContent = 'Registar Nova Peça';
    document.getElementById('formPeca').reset();
    const idInput = document.getElementById('pecaEdicaoId');
    if (idInput) idInput.value = '';
    document.getElementById('modalPeca').style.display = 'flex';
}

async function abrirModalEditarPeca(id) {
    try {
        const token = localStorage.getItem('token');

        const response = await fetch(`http://localhost:5039/api/pecas/${id}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': token ? `Bearer ${token}` : ''
            }
        });

        if (!response.ok) {
            throw new Error('Erro ao obter dados da peça.');
        }

        const peca = await response.json();

        const inputId = document.getElementById('pecaEdicaoId') || document.getElementById('pecaEdicaoReferencia');
        if (inputId) inputId.value = id;

        const tituloModal = document.getElementById('tituloModalPeca');
        if (tituloModal) tituloModal.textContent = 'Editar Peça';

        const setVal = (idEl, val) => {
            const el = document.getElementById(idEl);
            if (el) el.value = val ?? '';
        };

        setVal('referenciaPeca', peca.referenciaPeca ?? peca.ReferenciaPeca);
        setVal('nomePeca', peca.nome ?? peca.Nome);
        setVal('categoriaPeca', peca.categoria ?? peca.Categoria);
        setVal('compatibilidadePeca', peca.compatibilidade ?? peca.Compatibilidade);
        setVal('precoPeca', peca.precoUnitario ?? peca.PrecoUnitario ?? 0);
        setVal('stockPeca', peca.stockDisponivel ?? peca.StockDisponivel ?? 0);

        const modal = document.getElementById('modalPeca');
        if (modal) modal.style.display = 'flex';
    } catch (error) {
        console.error(error);
        alert('Não foi possível carregar os detalhes da peça.');
    }
}

function fecharModalPeca() {
    document.getElementById('modalPeca').style.display = 'none';
}

async function guardarPeca(event) {
    event.preventDefault();

    const getVal = (idEl) => document.getElementById(idEl)?.value ?? '';

    const idEdicao = getVal('pecaEdicaoId') || getVal('pecaEdicaoReferencia');
    const referenciaPeca = getVal('referenciaPeca');
    const nome = getVal('nomePeca');
    const categoria = getVal('categoriaPeca');
    const compatibilidade = getVal('compatibilidadePeca');
    const precoUnitario = parseFloat(getVal('precoPeca')) || 0;
    const stockDisponivel = parseInt(getVal('stockPeca')) || 0;

    const dadosPeca = {
        referenciaPeca,
        nome,
        categoria,
        compatibilidade,
        precoUnitario,
        stockDisponivel
    };

    const url = idEdicao
        ? `http://localhost:5039/api/pecas/${idEdicao}`
        : 'http://localhost:5039/api/pecas';

    const method = idEdicao ? 'PUT' : 'POST';

    try {
        const token = localStorage.getItem('token');
        console.log('A enviar pedido:', { url, method, token, dadosPeca });

        const headers = {
            'Content-Type': 'application/json'
        };

        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        const response = await fetch(url, {
            method: method,
            headers: headers,
            body: JSON.stringify(dadosPeca)
        });

        if (!response.ok) {
            const erroTexto = await response.text();
            throw new Error(erroTexto || 'Erro ao guardar a peça.');
        }

        fecharModalPeca();
        carregarPecas();
        await carregarPecasParaCache();
        alert('Peça guardada com sucesso!');

    } catch (error) {
        console.error('Erro ao guardar peça:', error);
        alert('Erro ao guardar a peça: ' + error.message);
    }
}

async function alternarInativarPeca(id, estadoAtual) {
    const acao = estadoAtual ? 'inativar' : 'ativar';
    if (!confirm(`Queres ${acao} esta peça?`)) return;

    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`http://localhost:5039/api/pecas/${id}/${acao}`, {
            method: 'PATCH',
            headers: {
                'Authorization': token ? `Bearer ${token}` : '',
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            alert('Erro ao alterar o estado da peça.');
            return;
        }

        carregarPecas();
    } catch (error) {
        console.error(error);
        alert('Erro de comunicação.');
    }
}

async function eliminarPeca(id) {
    if (!confirm('Tens a certeza que pretendes eliminar esta peça?')) {
        return;
    }

    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`http://localhost:5039/api/pecas/${id}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': token ? `Bearer ${token}` : ''
            }
        });

        if (!response.ok) {
            throw new Error('Falha ao eliminar a peça.');
        }

        carregarPecas();
        alert('Peça eliminada com sucesso!');
    } catch (error) {
        console.error('Erro ao eliminar:', error);
        alert('Não foi possível eliminar a peça.');
    }
}

function preencherSelectsPecas() {
    const selects = document.querySelectorAll('.select-peca, #pecaIdSelect');
    if (selects.length === 0 || !window.cachePecas) return;

    selects.forEach(select => {
        const valorAtual = select.value;
        select.innerHTML = '<option value="">Selecione uma peça...</option>';

        window.cachePecas.forEach(peca => {
            const id = peca.id || peca.Id;
            const nome = peca.nome || peca.Nome;
            const referencia = peca.referenciaPeca || peca.ReferenciaPeca || '';
            const stock = peca.stockDisponivel ?? peca.StockDisponivel ?? 0;

            const option = document.createElement('option');
            option.value = id;
            option.textContent = `${referencia ? referencia + ' - ' : ''}${nome} (Stock: ${stock})`;
            select.appendChild(option);
        });

        if (valorAtual) select.value = valorAtual;
    });
}

async function carregarVeiculos() {
    const tabela = document.getElementById('tabelaVeiculos');
    if (!tabela) return;
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

let clientesCarregados = []; // Declara no topo do teu ficheiro ou escopo global

async function carregarClientes() {
    try {
        const response = await fetch('https://localhost:7194/api/Auth/users', {
            credentials: 'include'
        });

        if (!response.ok) throw new Error('Erro ao carregar utilizadores');

        const utilizadores = await response.json();

        // Filtra e guarda na variável global
        clientesCarregados = utilizadores.filter(u => u.role && u.role.toLowerCase() === 'cliente');

        const tabelaClientes = document.getElementById('tabelaClientesBody');
        tabelaClientes.innerHTML = clientesCarregados.map(cliente => `
            <tr>
                <td class="user-id">${escaparHtml(cliente.id)}</td>
                <td>${escaparHtml(cliente.firstName)}</td>
                <td>${escaparHtml(cliente.email)}</td>
                <td>${escaparHtml(cliente.role)}</td>
                <td>
                    <button class="btn-action btn-edit" onclick="abrirModalEditarCliente('${cliente.id}')">
                        <i class="fa-solid fa-pen"></i> Editar
                    </button>
                    <button class="btn-action btn-delete" onclick="eliminarCliente('${cliente.id}')">
                        <i class="fa-solid fa-trash"></i> Eliminar
                    </button>
                </td>
            </tr>
        `).join('');

    } catch (error) {
        console.error("Erro ao carregar clientes:", error);
    }
}

document.addEventListener('DOMContentLoaded', carregarClientes);

let adminsCarregados = [];

async function carregarAdmins() {
    const tabela = document.getElementById('tabelaAdmins');
    if (!tabela) return;
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

let paginaAtualOrdens = 1;
let totalPaginasOrdens = 1;

let paginaAtualHistorico = 1;
let totalPaginasHistorico = 1;

let filtroVeiculoId = null;

function mudarPaginaOrdens(direcao) {
    const novaPagina = paginaAtualOrdens + direcao;
    if (novaPagina < 1 || novaPagina > totalPaginasOrdens) return;
    paginaAtualOrdens = novaPagina;
    carregarDadosDashboard();
}

function mudarPaginaHistorico(direcao) {
    const novaPagina = paginaAtualHistorico + direcao;
    if (novaPagina < 1 || novaPagina > totalPaginasHistorico) return;
    paginaAtualHistorico = novaPagina;
    carregarDadosDashboard();
}

function aplicarFiltroVeiculo() {
    const valor = document.getElementById('filtroVeiculoId').value;

    if (!valor || Number(valor) <= 0) {
        alert('Introduz um ID de veículo válido.');
        return;
    }

    filtroVeiculoId = Number(valor);
    paginaAtualOrdens = 1;
    paginaAtualHistorico = 1;
    carregarDadosDashboard();
}

function limparFiltroVeiculo() {
    document.getElementById('filtroVeiculoId').value = '';
    filtroVeiculoId = null;
    paginaAtualOrdens = 1;
    paginaAtualHistorico = 1;
    carregarDadosDashboard();
}

async function carregarDadosDashboard() {
    try {
        // 1. Carregar Pedidos Aprovados (Aguardar Criação de Ordem)
        const responsePedidos = await fetch('https://localhost:7085/api/pedidos/todos', {
            method: 'GET',
            credentials: 'include'
        });

        const tbodyOrdens = document.getElementById('tabelaOrdens');
        let pedidosAprovados = [];

        if (responsePedidos.ok && tbodyOrdens) {
            const dataPedidos = await responsePedidos.json();
            const todosPedidos = Array.isArray(dataPedidos) ? dataPedidos : (dataPedidos.itens || dataPedidos.pedidos || []);

            pedidosAprovados = todosPedidos.filter(p => (p.estado || p.Estado || '').toLowerCase() === 'aprovado');

            if (filtroVeiculoId !== null) {
                pedidosAprovados = pedidosAprovados.filter(p => Number(p.veiculoId ?? p.VeiculoId) === filtroVeiculoId);
            }

            if (pedidosAprovados.length === 0) {
                tbodyOrdens.innerHTML = `
                <tr>
                    <td colspan="7" style="text-align: center; padding: 25px;">
                        Não existem pedidos aprovados a aguardar criação de ordem.
                    </td>
                </tr>`;
            } else {
                tbodyOrdens.innerHTML = pedidosAprovados.map(pedido => {
                    const id = pedido.id ?? pedido.Id;
                    const cliente = escaparHtml(pedido.clienteId ?? pedido.ClienteId ?? '');
                    const veiculo = escaparHtml(pedido.veiculoId ?? pedido.VeiculoId ?? '');
                    const descricao = escaparHtml(pedido.descricaoProblema ?? pedido.problemaReportado ?? pedido.descricao ?? '-');

                    const badgeHtml = `
                        <span class="badge curso" style="background-color: #cce5ff; color: #004085; padding: 4px 8px; border-radius: 4px;">
                            <i class="fa-solid fa-clock" style="font-size: 10px;"></i> Aprovado
                        </span>`;

                    const acaoHtml = `
                        <button class="btn-action" style="background-color: #0d6efd; color: white; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer;" 
                                onclick="abrirModalCriarOrdemDePedido(${id}, '${cliente}', '${veiculo}', '${descricao.replace(/'/g, "\\'")}')">
                            <i class="fa-solid fa-plus"></i> Criar Ordem
                        </button>`;

                    return `
                        <tr>
                            <td><strong>#${id}</strong></td>
                            <td>${cliente}</td>
                            <td>${veiculo}</td>
                            <td>${descricao}</td>
                            <td><strong>-</strong></td>
                            <td>${badgeHtml}</td>
                            <td>${acaoHtml}</td>
                        </tr>
                    `;
                }).join('');
            }
        }

        // 2. Carregar Ordens de Reparação com Paginação e Filtro no Servidor
        let urlOrdens = `https://localhost:7085/api/OrdensReparacao?pagina=${paginaAtualHistorico}&tamanhoPagina=20`;
        if (filtroVeiculoId !== null) {
            urlOrdens += `&veiculoId=${filtroVeiculoId}`;
        }

        const responseOrdens = await fetch(urlOrdens, {
            method: 'GET',
            credentials: 'include'
        });

        const tbodyHistorico = document.getElementById('tabelaHistoricoOrdens');

        if (responseOrdens.ok && tbodyHistorico) {
            const dataOrdens = await responseOrdens.json();

            const listaOrdens = dataOrdens.itens || dataOrdens.Itens || [];
            totalPaginasHistorico = dataOrdens.totalPaginas ?? dataOrdens.TotalPaginas ?? 1;

            if (listaOrdens.length === 0) {
                tbodyHistorico.innerHTML = `
                <tr>
                    <td colspan="7" style="text-align: center; padding: 25px;">
                        Não existem ordens de reparação registadas.
                    </td>
                </tr>`;
            } else {
                tbodyHistorico.innerHTML = listaOrdens.map(ordem => {
                    const idOrdem = ordem.id ?? ordem.Id;
                    const cliente = escaparHtml(ordem.clienteId ?? ordem.ClienteId ?? '');
                    const veiculo = escaparHtml(ordem.veiculoId ?? ordem.VeiculoId ?? '');
                    const descricao = escaparHtml(ordem.descricaoProblema ?? ordem.DescricaoProblema ?? '-');
                    const estado = ordem.estado ?? ordem.Estado ?? 'Em Curso';
                    const valorTotal = ordem.valorTotal ?? ordem.ValorTotal ?? 0;

                    const isConcluida = estado.toLowerCase().includes('concluíd') || estado.toLowerCase().includes('concluid');
                    const badgeHtml = isConcluida ? `
                        <span class="badge concluida" style="background-color: #d1e7dd; color: #0f5132; padding: 4px 8px; border-radius: 4px;">
                            <i class="fa-solid fa-check" style="font-size: 10px;"></i> Concluída
                        </span>` : `
                        <span class="badge curso" style="background-color: #fff3cd; color: #664d03; padding: 4px 8px; border-radius: 4px;">
                            <i class="fa-solid fa-clock" style="font-size: 10px;"></i> Em Curso
                        </span>`;

                    const acaoHtml = `
                        <button class="btn-action" onclick="verDetalhesOrdem(${idOrdem})">
                            <i class="fa-solid fa-eye"></i> Ver
                        </button>`;

                    return `
                        <tr>
                            <td><strong>#${idOrdem}</strong></td>
                            <td>${cliente}</td>
                            <td>${veiculo}</td>
                            <td>${descricao}</td>
                            <td><strong>${formatarMoeda(valorTotal)}</strong></td>
                            <td>${badgeHtml}</td>
                            <td>${acaoHtml}</td>
                        </tr>
                    `;
                }).join('');
            }

            const infoPaginaHistoricoEl = document.getElementById('infoPaginaHistorico');
            if (infoPaginaHistoricoEl) {
                infoPaginaHistoricoEl.textContent = `Página ${paginaAtualHistorico} de ${totalPaginasHistorico}`;
            }

            const btnAntHist = document.getElementById('btnPaginaAnteriorHistorico');
            const btnSegHist = document.getElementById('btnPaginaSeguinteHistorico');
            if (btnAntHist) btnAntHist.disabled = paginaAtualHistorico <= 1;
            if (btnSegHist) btnSegHist.disabled = paginaAtualHistorico >= totalPaginasHistorico;

            const totalConcluidas = dataOrdens.totalConcluidas ?? dataOrdens.TotalConcluidas ?? 0;
            const totalEmCurso = pedidosAprovados.length;
            const totalGeralOrdens = totalConcluidas + totalEmCurso;

            if (document.getElementById('concluidas')) document.getElementById('concluidas').textContent = totalConcluidas;
            if (document.getElementById('emCurso')) document.getElementById('emCurso').textContent = totalEmCurso;
            if (document.getElementById('totalOrdens')) document.getElementById('totalOrdens').textContent = totalGeralOrdens;
        }

    } catch (error) {
        console.error('Erro ao carregar dados do dashboard:', error);
    }
}

let pedidoIdAtualParaOrdem = null;

function abrirModalCriarOrdemDePedido(pedidoId, clienteId, veiculoId, descricao) {
    pedidoIdAtualParaOrdem = pedidoId;

    const inputCliente = document.getElementById('clienteIdInput');
    const inputVeiculo = document.getElementById('veiculoIdInput');
    const inputDescricao = document.getElementById('descricaoInput');
    const inputQtdPecas = document.getElementById('quantidadePecasInput');

    if (inputCliente) inputCliente.value = clienteId;
    if (inputVeiculo) inputVeiculo.value = veiculoId;
    if (inputDescricao) inputDescricao.value = descricao;

    if (inputQtdPecas) inputQtdPecas.value = 0;
    gerarCamposPecas(0);

    const mensagemDiv = document.getElementById('mensagemOrdem');
    if (mensagemDiv) {
        mensagemDiv.textContent = '';
        mensagemDiv.className = 'mensagem-ordem';
    }

    const modal = document.getElementById('modalNovaOrdem');
    if (modal) modal.style.display = 'flex';
}

let pecasDisponiveisCache = [];

async function carregarPecasParaCache() {
    try {
        const token = localStorage.getItem('token');

        const response = await fetch('http://localhost:5039/api/pecas', {
            method: 'GET',
            headers: {
                'Authorization': token ? `Bearer ${token}` : '',
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Falha ao carregar peças para cache.');
        }

        const data = await response.json();
        window.cachePecas = Array.isArray(data) ? data : (data.value || data.itens || []);

        preencherSelectsPecas();
    } catch (error) {
        console.error('Erro ao carregar peças para o select:', error);
    }
}

carregarPecasParaCache();

async function gerarCamposPecas(quantidade) {
    const wrapper = document.getElementById('listaCamposPecas');
    if (!wrapper) return;
    wrapper.innerHTML = '';

    const qtd = parseInt(quantidade) || 0;

    if (!window.cachePecas || window.cachePecas.length === 0) {
        await carregarPecasParaCache();
    }

    let optionsHtml = '<option value="">Selecione uma peça...</option>';

    const stockPorPeca = {};

    window.cachePecas.forEach(p => {
        const ativo = p.ativo ?? p.Ativo ?? true;
        if (!ativo) return;

        const idUnico = p.id || p.Id;
        const nomePeca = p.nome || p.Nome;
        const referencia = p.referenciaPeca || p.ReferenciaPeca || '';
        const stock = p.stockDisponivel ?? p.StockDisponivel ?? 0;

        stockPorPeca[idUnico] = stock;

        optionsHtml += `<option value="${idUnico}">${referencia ? referencia + ' - ' : ''}${nomePeca} (Stock: ${stock})</option>`;
    });

    for (let i = 0; i < qtd; i++) {
        wrapper.innerHTML += `
            <div class="pecas-item-container" style="border-top: 1px dashed #ddd; padding-top: 10px; margin-top: 10px;">
                <div style="font-size: 12px; font-weight: 600; color: #555; margin-bottom: 5px;">Peça ${i + 1}</div>
                <div style="display: flex; gap: 10px;">
                    <div style="flex: 2;">
                        <label style="font-size: 11px; color: #666; display: block; margin-bottom: 2px;">Peça</label>
                        <select class="form-control form-control-sm peca-select" required style="width: 100%; padding: 6px;" onchange="atualizarLimiteStock(this, ${JSON.stringify(stockPorPeca)})">
                            ${optionsHtml}
                        </select>
                    </div>
                    <div style="flex: 1;">
                        <label style="font-size: 11px; color: #666; display: block; margin-bottom: 2px;">Qtd</label>
                        <input type="number" class="form-control form-control-sm peca-qtd" placeholder="Qtd" min="1" value="1" required style="width: 100%; padding: 6px;">
                    </div>
                </div>
            </div>
        `;
    }
}

function atualizarLimiteStock(selectEl, stockMap) {
    const container = selectEl.closest('.pecas-item-container');
    const inputQtd = container.querySelector('.peca-qtd');
    const idSelecionado = selectEl.value;

    if (idSelecionado && stockMap[idSelecionado] !== undefined) {
        const stockMaximo = stockMap[idSelecionado];
        inputQtd.max = stockMaximo;

        if (parseInt(inputQtd.value) > stockMaximo) {
            inputQtd.value = stockMaximo;
        }
    } else {
        inputQtd.removeAttribute('max');
    }
}

async function criarOrdemReparacao(event) {
    event.preventDefault();

    const mensagemDiv = document.getElementById('mensagemOrdem');
    if (mensagemDiv) {
        mensagemDiv.textContent = "A processar registo...";
        mensagemDiv.className = "mensagem-ordem info";
    }

    try {
        const clienteId = document.getElementById('clienteIdInput').value;
        const veiculoId = document.getElementById('veiculoIdInput').value;
        const custoMaoDeObra = parseFloat(document.getElementById('custoMaoDeObraInput').value) || 0;
        const descricao = document.getElementById('descricaoInput').value;

        const pecasArray = [];
        const containersPecas = document.querySelectorAll('.pecas-item-container');

        for (const container of containersPecas) {
            const selectPeca = container.querySelector('.peca-select');
            const inputQtd = container.querySelector('.peca-qtd');

            if (selectPeca && inputQtd && selectPeca.value) {
                const pecaIdSelecionada = String(selectPeca.value).trim().toLowerCase();
                const quantidadePedida = parseInt(inputQtd.value) || 1;

                const pecaInfo = window.cachePecas.find(p => {
                    const idOriginal = String(p.id || p.Id || '').trim().toLowerCase();
                    return idOriginal === pecaIdSelecionada;
                });

                const stockDisponivel = pecaInfo ? Number(pecaInfo.stockDisponivel ?? pecaInfo.StockDisponivel ?? 0) : 0;

                if (quantidadePedida > stockDisponivel) {
                    const textoErro = `Erro: A quantidade pedida (${quantidadePedida}) excede o stock disponível (${stockDisponivel}) para a peça selecionada.`;

                    if (mensagemDiv) {
                        mensagemDiv.textContent = textoErro;
                        mensagemDiv.className = "mensagem-ordem error";
                    }

                    alert(textoErro);
                    return;
                }

                pecasArray.push({
                    pecaId: selectPeca.value,
                    quantidade: quantidadePedida
                });
            }
        }

        const payload = {
            clienteId: clienteId,
            veiculoId: parseInt(veiculoId) || 0,
            custoMaoDeObra: custoMaoDeObra,
            descricaoProblema: descricao,
            dataConclusao: document.getElementById('dataConclusaoInput')?.value ? new Date(document.getElementById('dataConclusaoInput').value).toISOString() : new Date().toISOString(),
            pecas: pecasArray
        };

        const response = await fetch('https://localhost:7085/api/ordensreparacao', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include',
            body: JSON.stringify(payload)
        });

        if (response.ok) {
            if (pedidoIdAtualParaOrdem) {
                await fetch(`https://localhost:7085/api/pedidos/${pedidoIdAtualParaOrdem}/analisar`, {
                    method: 'PATCH',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    credentials: 'include',
                    body: JSON.stringify({
                        estado: 'Completo',
                        observacoes: 'Ordem de reparação criada com sucesso.'
                    })
                }).catch(err => console.error("Erro ao atualizar estado do pedido:", err));
            }

            if (mensagemDiv) {
                mensagemDiv.textContent = "Ordem de reparação criada com sucesso!";
                mensagemDiv.className = "mensagem-ordem success";
            }

            setTimeout(() => {
                fecharModalNovaOrdem();
                carregarDadosDashboard();
            }, 1200);

        } else {
            const erroTexto = await response.text();
            if (mensagemDiv) {
                mensagemDiv.textContent = "Erro ao criar ordem: " + (erroTexto || response.statusText);
                mensagemDiv.className = "mensagem-ordem error";
            }
        }

    } catch (error) {
        console.error("Erro de rede ao submeter ordem:", error);
        if (mensagemDiv) {
            mensagemDiv.textContent = "Erro de ligação ao servidor.";
            mensagemDiv.className = "mensagem-ordem error";
        }
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
            const welcomeEl = document.getElementById('welcomeMessage');
            if (welcomeEl) {
                welcomeEl.textContent = `Bem-vindo, ${data.firstName}!`;
            }
        }

    } catch (error) {
        console.error('Não foi possível carregar o nome do utilizador:', error);
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

let ordemDetalheAtual = null;

function fecharModalDetalhes() {
    document.getElementById('modalDetalhesOrdem').style.display = 'none';
}

function preencherDetalhesOrdem(ordem) {
    const id = ordem.id ?? ordem.Id;
    document.getElementById('tituloDetalheOrdem').textContent = `Detalhes da Ordem #${id}`;

    document.getElementById('detalheCliente').textContent = ordem.clienteId ?? ordem.ClienteId ?? '-';
    document.getElementById('detalheVeiculo').textContent = `#${ordem.veiculoId ?? ordem.VeiculoId ?? '-'}`;
    document.getElementById('detalheDataEntrada').textContent = formatarData(ordem.dataEntrada ?? ordem.DataEntrada);
    document.getElementById('detalheDataConclusao').textContent = formatarData(ordem.dataConclusao ?? ordem.DataConclusao);

    const maoDeObra = ordem.custoMaoDeObra ?? ordem.CustoMaoDeObra ?? 0;
    const custoPecas = ordem.custoPecas ?? ordem.CustoPecas ?? 0;
    const total = ordem.valorTotal ?? ordem.ValorTotal ?? (maoDeObra + custoPecas);

    document.getElementById('detalheMaoDeObra').textContent = formatarMoeda(maoDeObra);
    document.getElementById('detalheCustoPecas').textContent = formatarMoeda(custoPecas);
    document.getElementById('detalheTotal').textContent = formatarMoeda(total);
    document.getElementById('detalheDescricao').textContent = ordem.descricaoProblema ?? ordem.DescricaoProblema ?? '-';

    const selectEstado = document.getElementById('estadoOrdem');
    if (selectEstado) {
        const grupoFormulario = selectEstado.closest('.form-group') || selectEstado.parentElement;
        if (grupoFormulario) grupoFormulario.remove();
    }

    const btnGuardarEstado = document.querySelector('button[onclick="alterarEstadoOrdem()"]');
    if (btnGuardarEstado) {
        const containerBtn = btnGuardarEstado.closest('div') || btnGuardarEstado.parentElement;
        if (containerBtn) containerBtn.remove();
    }

    const listaPecas = document.getElementById('listaPecasDetalhe');
    const pecasArray = ordem.pecas ?? ordem.Pecas ?? [];

    if (!pecasArray || pecasArray.length === 0) {
        listaPecas.innerHTML = '<p>Sem peças registadas nesta ordem.</p>';
        return;
    }

    listaPecas.innerHTML = pecasArray.map(pecaItem => {
        const pId = pecaItem.pecaId ?? pecaItem.PecaId ?? '';
        const qtd = pecaItem.quantidade ?? pecaItem.Quantidade ?? 0;
        const precoU = pecaItem.precoUnitario ?? pecaItem.PrecoUnitario ?? 0;
        const sub = pecaItem.subtotal ?? (qtd * precoU);

        const pecaIdBuscado = String(pId).trim().toLowerCase();
        const pecaInfo = window.cachePecas?.find(p => String(p.id || p.Id || '').trim().toLowerCase() === pecaIdBuscado);
        const nomePeca = pecaInfo ? (pecaInfo.nome || pecaInfo.Nome) : pId;

        return `
            <div class="peca-detalhe" style="display: flex; justify-content: space-between; margin-bottom: 8px; border-bottom: 1px solid #eee; padding-bottom: 4px;">
                <span><strong>${escaparHtml(nomePeca)}</strong></span>
                <span>${qtd} un. × ${formatarMoeda(precoU)}</span>
                <strong>${formatarMoeda(sub)}</strong>
            </div>
        `;
    }).join('');
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
            mensagem.textContent = dados.mensagem || 'Não foi possível alterar o estado.';
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
        document.getElementById('mensagemEditarOrdem').className = 'mensagem-ordem';

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
        custoMaoDeObra: Number(document.getElementById('editarMaoDeObraOrdem').value),
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
    if (mensagem) {
        mensagem.textContent = '';
        mensagem.className = 'mensagem-ordem';
    }
}

function abrirModalNovoAdmin() {
    document.getElementById('adminEdicaoId').value = '';
    document.getElementById('adminPrimeiroNome').value = '';
    document.getElementById('adminEmail').value = '';
    document.getElementById('adminPassword').value = '';

    const confirmInput = document.getElementById('adminConfirmPassword');
    if (confirmInput) {
        confirmInput.value = '';
        confirmInput.required = true; // Obrigatório ao criar novo
    }
    document.getElementById('adminPassword').required = true;

    document.getElementById('tituloModalAdmin').textContent = 'Novo Administrador';
    document.getElementById('labelPasswordAdmin').textContent = 'Password';
    document.getElementById('ajudaPasswordAdmin').textContent = 'Obrigatória ao criar um novo administrador.';

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

    // NOVO: Limpa o campo de confirmação e torna-o opcional na edição
    const confirmInput = document.getElementById('adminConfirmPassword');
    if (confirmInput) {
        confirmInput.value = '';
        confirmInput.required = false;
    }

    document.getElementById('adminPassword').required = false;

    document.getElementById('tituloModalAdmin').textContent = 'Editar Administrador';
    document.getElementById('labelPasswordAdmin').textContent = 'Nova password';
    document.getElementById('ajudaPasswordAdmin').textContent = 'Opcional. Deixa vazia para manter a password atual.';

    limparMensagemAdmin();
    document.getElementById('modalAdmin').style.display = 'flex';
}

async function guardarAdmin(event) {
    event.preventDefault();

    const id = document.getElementById('adminEdicaoId').value;
    const primeiroNome = document.getElementById('adminPrimeiroNome').value.trim();
    const email = document.getElementById('adminEmail').value.trim();
    const password = document.getElementById('adminPassword').value;

    const mensagem = document.getElementById('mensagemAdmin');

    if (!id && !password) {
        mensagem.textContent = 'A password é obrigatória ao criar um administrador.';
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
        mensagem.textContent = 'Erro de comunicação ao guardar o administrador.';
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

function abrirModalNovoCliente() {
    document.getElementById('clienteEdicaoId').value = '';
    document.getElementById('clientePrimeiroNome').value = '';
    document.getElementById('clienteEmail').value = '';
    document.getElementById('clientePassword').value = '';
    document.getElementById('clienteConfirmPassword').value = '';

    document.getElementById('tituloModalCliente').textContent = 'Novo Cliente';
    document.getElementById('labelPasswordCliente').textContent = 'Password';
    document.getElementById('ajudaPasswordCliente').textContent = 'Obrigatória ao criar um novo cliente.';

    // Torna as passwords obrigatórias novamente
    document.getElementById('clientePassword').required = true;
    document.getElementById('clienteConfirmPassword').required = true;

    const btnSubmit = document.querySelector('#formCliente .btn-submit-modal');
    if (btnSubmit) btnSubmit.textContent = 'Guardar Cliente';

    document.getElementById('modalCliente').style.display = 'flex';
}

function fecharModalCliente() {
    document.getElementById('modalCliente').style.display = 'none';
    document.getElementById('mensagemCliente').textContent = '';
}

function abrirModalEditarCliente(id) {
    const cliente = clientesCarregados.find(item => item.id === id);

    if (!cliente) {
        alert('Não foi possível encontrar o cliente.');
        return;
    }

    // Preenche os inputs com os dados corretos
    document.getElementById('clienteEdicaoId').value = cliente.id;
    document.getElementById('clientePrimeiroNome').value = cliente.firstName || '';
    document.getElementById('clienteEmail').value = cliente.email || '';
    document.getElementById('clientePassword').value = '';
    document.getElementById('clienteConfirmPassword').value = '';

    // Altera os textos e títulos para o modo de edição
    document.getElementById('tituloModalCliente').textContent = 'Editar Cliente';
    document.getElementById('labelPasswordCliente').textContent = 'Nova password';
    document.getElementById('ajudaPasswordCliente').textContent = 'Opcional. Deixa vazia para manter a password atual.';

    // Opcional: Se a password passa a ser opcional na edição, removemos o required
    document.getElementById('clientePassword').required = false;
    document.getElementById('clienteConfirmPassword').required = false;

    // Altera o texto do botão de submissão
    const btnSubmit = document.querySelector('#formCliente .btn-submit-modal');
    if (btnSubmit) btnSubmit.textContent = 'Atualizar Cliente';

    // Limpa mensagens de erro anteriores se tiveres essa função
    // limparMensagemCliente();

    // Mostra o modal com flex (para ficar centrado igual ao dos admins)
    document.getElementById('modalCliente').style.display = 'flex';
}

async function guardarCliente(e) {
    e.preventDefault();

    const id = document.getElementById('clienteEdicaoId').value;
    const firstName = document.getElementById('clientePrimeiroNome').value;
    const email = document.getElementById('clienteEmail').value;
    const password = document.getElementById('clientePassword').value;
    const confirmPassword = document.getElementById('clienteConfirmPassword').value;

    if (password && password !== confirmPassword) {
        alert('As passwords não coincidem.');
        return;
    }

    const dados = {
        firstName: firstName,
        email: email,
        password: password,
        confirmPassword: confirmPassword,
        role: "Cliente"
    };

    try {
        let url = 'https://localhost:7194/api/Auth/users';
        let method = id ? 'PUT' : 'POST';

        if (id) {
            url = `https://localhost:7194/api/Auth/users/${id}`;
        }

        const response = await fetch(url, {
            method: method,
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dados)
        });

        if (response.ok) {
            alert('Cliente guardado com sucesso!');
            fecharModalCliente();
            carregarClientes();
        } else {
            const errData = await response.json().catch(() => ({}));
            let msgErro = errData.message || '';
            if (!msgErro && typeof errData === 'object' && errData.errors) {
                msgErro = Object.values(errData.errors).flat().join(' | ');
            } else if (!msgErro && typeof errData === 'object') {
                msgErro = Object.values(errData).flat().join(' | ');
            }
            alert('Erro ao guardar cliente: ' + (msgErro || response.statusText));
        }
    } catch (error) {
        console.error("Erro de rede:", error);
        alert('Erro de comunicação com o servidor.');
    }
}

async function eliminarCliente(id) {
    if (!confirm('Tens a certeza de que pretendes eliminar este cliente?')) {
        return;
    }

    try {
        const response = await fetch(`https://localhost:7194/api/Auth/users/${id}`, {
            method: 'DELETE',
            credentials: 'include'
        });

        if (response.ok) {
            carregarClientes();
        } else {
            const errData = await response.json().catch(() => ({}));
            alert('Erro ao eliminar cliente: ' + (errData.message || 'Erro desconhecido.'));
        }
    } catch (error) {
        console.error(error);
        alert('Erro de comunicação ao eliminar o cliente.');
    }
}

carregarDadosDashboard();
carregarNomeUtilizador();

// Abre o modal de veículo, seguindo o mesmo padrão do modal de Peças.
function abrirModalNovoVeiculo() {
    document.getElementById('tituloModalVeiculo').textContent =
        'Registar Novo Veículo';

    document.getElementById('formVeiculo').reset();
    document.getElementById('veiculoEdicaoId').value = '';
    document.getElementById('mensagemVeiculo').textContent = '';

    document.getElementById('modalVeiculo').style.display = 'flex';
}

// Fecha o modal de veículo, seguindo o mesmo padrão do modal de Peças.
function fecharModalVeiculo() {
    document.getElementById('modalVeiculo').style.display = 'none';
}
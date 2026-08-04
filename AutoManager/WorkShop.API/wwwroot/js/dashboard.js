const urlParams = new URLSearchParams(window.location.search);
const tokenFromUrl = urlParams.get('token');

if (tokenFromUrl) {
    window.history.replaceState({}, document.title, window.location.pathname);
}

function terminarSessao() {
    window.location.href = 'https://localhost:7194/login.html';
}

function abrirModalNovaOrdem() {
    const modal = document.getElementById('modalNovaOrdem');
    if (modal) modal.style.display = 'flex';
}

function fecharModalNovaOrdem() {
    const modal = document.getElementById('modalNovaOrdem');
    if (modal) modal.style.display = 'none';
}

// Funções para gerir o Modal de Adicionar Veículo
function abrirModalVeiculo() {
    const modal = document.getElementById('modalNovoVeiculo');
    if (modal) modal.style.display = 'flex';
}

function fecharModalVeiculo() {
    const modal = document.getElementById('modalNovoVeiculo');
    if (modal) modal.style.display = 'none';
    const form = document.getElementById('formNovoVeiculo');
    if (form) form.reset();
}

const titulosSecoes = {
    dashboard: 'Painel Geral da Oficina',
    ordens: 'Ordens de Reparação',
    'meus-veiculos': 'Os Meus Veículos',
    veiculos: 'Veículos (Geral)',
    clientes: 'Clientes'
};

function mostrarSecao(secao) {
    const mostrarDashboard = secao === 'dashboard';
    const mostrarOrdens = secao === 'dashboard' || secao === 'ordens';

    const btnNovaOrdem = document.getElementById('btnNovaOrdem');
    if (btnNovaOrdem) {
        btnNovaOrdem.classList.toggle('view-hidden', secao !== 'ordens');
    }

    const tituloPagina = document.getElementById('tituloPagina');
    if (tituloPagina) {
        tituloPagina.textContent = titulosSecoes[secao] || 'Painel';
    }

    const dashboardCards = document.getElementById('dashboardCards');
    if (dashboardCards) {
        dashboardCards.classList.toggle('view-hidden', !mostrarDashboard);
    }

    const secaoOrdens = document.getElementById('secaoOrdens');
    if (secaoOrdens) {
        secaoOrdens.classList.toggle('view-hidden', !mostrarOrdens);
    }

    const secaoMeusVeiculos = document.getElementById('secaoMeusVeiculos');
    if (secaoMeusVeiculos) {
        secaoMeusVeiculos.classList.toggle('view-hidden', secao !== 'meus-veiculos');
    }

    const secaoVeiculos = document.getElementById('secaoVeiculos');
    if (secaoVeiculos) {
        secaoVeiculos.classList.toggle('view-hidden', secao !== 'veiculos');
    }

    const secaoClientes = document.getElementById('secaoClientes');
    if (secaoClientes) {
        secaoClientes.classList.toggle('view-hidden', secao !== 'clientes');
    }

    document.querySelectorAll('.menu li').forEach(item => {
        item.classList.toggle('active', item.dataset.section === secao);
    });

    if (secao === 'dashboard' || secao === 'ordens') {
        carregarDadosDashboard();
    } else if (secao === 'meus-veiculos') {
        carregarMeusVeiculos();
    } else if (secao === 'veiculos') {
        carregarVeiculos();
    } else if (secao === 'clientes') {
        carregarClientes();
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
            mostrarTabelaVazia('tabelaVeiculos', 6, 'Não existem veículos registados.');
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
        mostrarTabelaVazia('tabelaVeiculos', 6, 'Erro ao carregar os veículos.');
    }
}

async function carregarMeusVeiculos() {
    const tabela = document.getElementById('tabelaMeusVeiculos');
    if (!tabela) return;
    tabela.innerHTML = '';

    try {
        const response = await fetch('https://localhost:7085/api/Veiculos', {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error('Não foi possível carregar os teus veículos.');
        }

        const veiculos = await response.json();

        if (veiculos.length === 0) {
            mostrarTabelaVazia('tabelaMeusVeiculos', 5, 'Ainda não tens nenhum veículo registado.');
            return;
        }

        tabela.innerHTML = veiculos.map(veiculo => `
            <tr>
                <td><strong>#${veiculo.id}</strong></td>
                <td>${escaparHtml(veiculo.matricula)}</td>
                <td>${escaparHtml(veiculo.marca)}</td>
                <td>${escaparHtml(veiculo.modelo)}</td>
                <td>${veiculo.ano}</td>
            </tr>
        `).join('');
    } catch (error) {
        console.error(error);
        mostrarTabelaVazia('tabelaMeusVeiculos', 5, 'Erro ao carregar os teus veículos.');
    }
}

async function registarVeiculo(e) {
    e.preventDefault();

    const novoVeiculo = {
        matricula: document.getElementById('matriculaInput').value,
        marca: document.getElementById('marcaInput').value,
        modelo: document.getElementById('modeloInput').value,
        ano: parseInt(document.getElementById('anoInput').value)
    };

    try {
        const response = await fetch('https://localhost:7085/api/Veiculos', {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(novoVeiculo)
        });

        const mensagem = document.getElementById('mensagemVeiculo');

        if (response.ok) {
            if (mensagem) {
                mensagem.textContent = 'Veículo adicionado com sucesso!';
                mensagem.className = 'mensagem-ordem sucesso';
            }

            setTimeout(() => {
                fecharModalVeiculo();
                if (mensagem) {
                    mensagem.textContent = '';
                    mensagem.className = 'mensagem-ordem';
                }
                carregarMeusVeiculos();
            }, 2000);
        } else {
            const errData = await response.json().catch(() => ({}));
            if (mensagem) {
                mensagem.textContent = errData.message || 'Erro ao registar o veículo.';
                mensagem.className = 'mensagem-ordem erro';
            }
        }
    } catch (error) {
        console.error('Erro:', error);
        alert('Erro de comunicação ao registar o veículo.');
    }
}

async function carregarClientes() {
    const tabela = document.getElementById('tabelaClientes');
    if (!tabela) return;
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
            mostrarTabelaVazia('tabelaClientes', 4, 'Não existem clientes registados.');
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
        mostrarTabelaVazia('tabelaClientes', 4, 'Erro ao carregar os clientes.');
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
    const inputFiltro = document.getElementById('filtroVeiculoId');
    if (!inputFiltro) return;
    const valor = inputFiltro.value;

    if (!valor || Number(valor) <= 0) {
        alert('Introduz um ID de veículo válido.');
        return;
    }

    filtroVeiculoId = Number(valor);
    paginaAtual = 1;
    carregarDadosDashboard();
}

function limparFiltroVeiculo() {
    const inputFiltro = document.getElementById('filtroVeiculoId');
    if (inputFiltro) inputFiltro.value = '';
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
        if (!tbody) return;

        tbody.innerHTML = '';

        if (!dados.itens || dados.itens.length === 0) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="7" style="text-align: center; padding: 25px;">
                        Não existem ordens de reparação.
                    </td>
                </tr>
            `;
            return;
        }

        dados.itens.forEach(ordem => {
            let estado = ordem.estado || 'Em Curso';
            let estadoClasse = estado === 'Em Curso' ? 'curso' : (estado === 'Pendente' ? 'pendente' : 'concluida');

            const tr = document.createElement('tr');

            let botaoAcaoHtml = `
                <button class="btn-action" onclick="verDetalhesOrdem(${ordem.id})">
                    <i class="fa-solid fa-eye"></i> Ver
                </button>
            `;

            if (window.isUtilizadorCliente && estado === 'Pendente') {
                botaoAcaoHtml += `
                    <button class="btn-action btn-aceitar" onclick="aceitarOrdemReparacao(${ordem.id})" style="background-color: #28a745; color: white; margin-left: 5px;">
                        <i class="fa-solid fa-check"></i> Aceitar
                    </button>
                `;
            }

            tr.innerHTML = `
                <td><strong>#${ordem.id}</strong></td>
                <td>${escaparHtml(ordem.clienteId)}</td>
                <td>${escaparHtml(ordem.veiculoId)}</td>
                <td>${escaparHtml(ordem.descricaoProblema)}</td>
                <td><strong>${Number(ordem.valorTotal || 0).toFixed(2)} €</strong></td>
                <td>
                    <span class="badge ${estadoClasse}">
                        <i class="fa-solid fa-circle" style="font-size: 6px;"></i>
                        ${escaparHtml(estado)}
                    </span>
                </td>
                <td>${botaoAcaoHtml}</td>
            `;

            tbody.appendChild(tr);
        });

        const elTotal = document.getElementById('totalOrdens');
        const elCurso = document.getElementById('emCurso');
        const elConcluidas = document.getElementById('concluidas');

        if (elTotal) elTotal.textContent = dados.totalOrdens ?? 0;
        if (elCurso) elCurso.textContent = dados.totalEmCurso ?? 0;
        if (elConcluidas) elConcluidas.textContent = dados.totalConcluidas ?? 0;

        paginaAtual = dados.paginaAtual;
        totalPaginas = dados.totalPaginas;

        const infoPagina = document.getElementById('infoPagina');
        if (infoPagina) {
            infoPagina.textContent = `Página ${paginaAtual} de ${totalPaginas} (${dados.totalItens || 0} ordem(ns))`;
        }

        const btnAnt = document.getElementById('btnPaginaAnterior');
        const btnSeg = document.getElementById('btnPaginaSeguinte');
        if (btnAnt) btnAnt.disabled = paginaAtual === 1;
        if (btnSeg) btnSeg.disabled = paginaAtual === totalPaginas;

    } catch (error) {
        console.error('Erro de ligação:', error);
    }
}

async function aceitarOrdemReparacao(ordemId) {
    if (!confirm('Tens a certeza de que pretendes aceitar esta ordem de reparação?')) return;

    try {
        const response = await fetch(`https://localhost:7085/api/OrdensReparacao/${ordemId}/aceitar`, {
            method: 'PUT',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            alert('Ordem aceite com sucesso! Passou para o estado Em Curso.');
            carregarDadosDashboard();
        } else {
            alert('Não foi possível aceitar a ordem.');
        }
    } catch (error) {
        console.error('Erro:', error);
        alert('Erro de comunicação com o servidor.');
    }
}

async function criarOrdemReparacao(e) {
    e.preventDefault();

    const pecaIdInput = document.getElementById('pecaIdInput');
    const qtdInput = document.getElementById('quantidadePecaInput');
    const clienteInput = document.getElementById('clienteIdInput');
    const veiculoInput = document.getElementById('veiculoIdInput');
    const maoObraInput = document.getElementById('custoMaoDeObraInput');
    const descInput = document.getElementById('descricaoInput');

    const pecaIdVal = pecaIdInput ? pecaIdInput.value : '';
    const qtdVal = qtdInput ? parseInt(qtdInput.value) || 0 : 0;

    let pecasArray = [];
    if (pecaIdVal && qtdVal > 0) {
        pecasArray.push({
            pecaId: pecaIdVal,
            quantidade: qtdVal
        });
    }

    const novaOrdem = {
        clienteId: clienteInput ? clienteInput.value : '',
        veiculoId: veiculoInput ? parseInt(veiculoInput.value) : 0,
        custoMaoDeObra: maoObraInput ? parseFloat(maoObraInput.value) : 0,
        descricaoProblema: descInput ? descInput.value : '',
        pecas: pecasArray
    };

    try {
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
            if (mensagem) {
                mensagem.textContent = 'Ordem de reparação criada como pendente!';
                mensagem.className = 'mensagem-ordem sucesso';
            }

            setTimeout(() => {
                fecharModalNovaOrdem();
                const formOrdem = document.getElementById('formNovaOrdem');
                if (formOrdem) formOrdem.reset();

                if (mensagem) {
                    mensagem.textContent = '';
                    mensagem.className = 'mensagem-ordem';
                }

                carregarDadosDashboard();
            }, 3000);
        } else {
            const errData = await response.json().catch(() => ({}));
            alert('Erro ao criar ordem: ' + (errData.mensagem || errData.message || 'Verifique os dados inseridos.'));
        }
    } catch (err) {
        console.error(err);
        alert('Erro de comunicação ao submeter a ordem.');
    }
}

window.isUtilizadorCliente = false;

async function carregarNomeUtilizador() {
    try {
        const response = await fetch('https://localhost:7194/api/Auth/me', {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) return;

        const data = await response.json();

        const welcomeMessage = document.getElementById('welcomeMessage');
        if (welcomeMessage && data.firstName) {
            welcomeMessage.textContent = `Bem-vindo, ${data.firstName}!`;
        }

        let userRole = data.role || data.tipoConta || '';
        if (!userRole && data.roles && data.roles.length > 0) {
            userRole = data.roles[0];
        }

        const eCliente = String(userRole).toLowerCase().trim() === 'cliente';
        window.isUtilizadorCliente = eCliente;

        if (eCliente) {
            const menuVeiculosGeral = document.querySelector('[data-section="veiculos"]');
            const menuClientes = document.querySelector('[data-section="clientes"]');

            if (menuVeiculosGeral) menuVeiculosGeral.style.display = 'none';
            if (menuClientes) menuClientes.style.display = 'none';

            const btnNovaOrdem = document.getElementById('btnNovaOrdem');
            if (btnNovaOrdem) {
                btnNovaOrdem.style.display = 'none';
            }

            // Aterra por defeito na secção de gerir os seus veículos
            mostrarSecao('meus-veiculos');

            const menuDashboard = document.querySelector('[data-section="dashboard"]');
            if (menuDashboard) menuDashboard.style.display = 'none';
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
    const modal = document.getElementById('modalDetalhesOrdem');
    if (modal) modal.style.display = 'none';
}

function preencherDetalhesOrdem(ordem) {
    const titulo = document.getElementById('tituloDetalheOrdem');
    if (titulo) titulo.textContent = `Detalhes da Ordem #${ordem.id}`;

    const setContent = (id, text) => {
        const el = document.getElementById(id);
        if (el) el.textContent = text;
    };

    setContent('detalheCliente', ordem.clienteId);
    setContent('detalheVeiculo', `#${ordem.veiculoId}`);
    setContent('detalheDataEntrada', formatarData(ordem.dataEntrada));
    setContent('detalheDataConclusao', formatarData(ordem.dataConclusao));
    setContent('detalheMaoDeObra', formatarMoeda(ordem.custoMaoDeObra));
    setContent('detalheCustoPecas', formatarMoeda(ordem.custoPecas));
    setContent('detalheTotal', formatarMoeda(ordem.valorTotal));
    setContent('detalheDescricao', ordem.descricaoProblema);

    const estadoOrdem = document.getElementById('estadoOrdem');
    if (estadoOrdem) estadoOrdem.value = ordem.estado;

    const listaPecas = document.getElementById('listaPecasDetalhe');
    if (listaPecas) {
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

        const modal = document.getElementById('modalDetalhesOrdem');
        if (modal) modal.style.display = 'flex';
    } catch (error) {
        console.error(error);
        alert('Erro de comunicação ao carregar os detalhes.');
    }
}

async function alterarEstadoOrdem() {
    if (!ordemDetalheAtual) return;

    const estadoInput = document.getElementById('estadoOrdem');
    const estado = estadoInput ? estadoInput.value : '';
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
            if (mensagem) {
                mensagem.textContent = dados.mensagem || 'Não foi possível alterar o estado.';
                mensagem.className = 'mensagem-ordem erro';
            }
            return;
        }

        ordemDetalheAtual = { ...ordemDetalheAtual, ...dados };
        preencherDetalhesOrdem(ordemDetalheAtual);

        if (mensagem) {
            mensagem.textContent = 'Estado atualizado com sucesso.';
            mensagem.className = 'mensagem-ordem sucesso';
        }

        carregarDadosDashboard();
    } catch (error) {
        console.error(error);
        if (mensagem) {
            mensagem.textContent = 'Erro de comunicação ao atualizar o estado.';
            mensagem.className = 'mensagem-ordem erro';
        }
    }
}

carregarDadosDashboard();
carregarNomeUtilizador();
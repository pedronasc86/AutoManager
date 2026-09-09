let paginaAtualCliente = 1;

// Paginação do histórico aberto no modal.
const pedidosPorPaginaHistorico = 5;
let paginaAtualHistoricoPedidos = 1;
let pedidosHistoricoCarregados = [];

function mostrarSecaoCliente(secao) {
    const secaoDashboard = document.getElementById('secaoDashboard');
    const secaoOrdens = document.getElementById('secaoOrdens');
    const secaoMeusVeiculos = document.getElementById('secaoMeusVeiculos');
    const secaoPecas = document.getElementById('secaoPecas');
    const secaoPedidos = document.getElementById('secaoPedidos');
    const meusVeiculosCards = document.getElementById('meusVeiculosCards');
    const tituloPagina = document.getElementById('tituloPagina');

    const mostrarDashboard = secao === 'dashboard';
    const mostrarOrdens = secao === 'ordens';
    const mostrarVeiculos = secao === 'meus-veiculos';
    const mostrarPecas = secao === 'pecas';
    const mostrarPedidos = secao === 'pedidos';

    if (secaoDashboard) secaoDashboard.classList.toggle('view-hidden', !mostrarDashboard);
    if (secaoOrdens) secaoOrdens.classList.toggle('view-hidden', !mostrarOrdens);
    if (secaoMeusVeiculos) secaoMeusVeiculos.classList.toggle('view-hidden', !mostrarVeiculos);
    if (secaoPecas) secaoPecas.classList.toggle('view-hidden', !mostrarPecas);
    if (secaoPedidos) secaoPedidos.classList.toggle('view-hidden', !mostrarPedidos);

    if (meusVeiculosCards) {
        meusVeiculosCards.classList.toggle('view-hidden', !mostrarVeiculos);
    }

    if (tituloPagina) {
        if (secao === 'meus-veiculos') tituloPagina.textContent = 'Os Meus Veículos';
        else if (secao === 'ordens') tituloPagina.textContent = 'As Minhas Reparações';
        else if (secao === 'pecas') tituloPagina.textContent = 'Peças Disponíveis em Stock';
        else if (secao === 'pedidos') tituloPagina.textContent = 'Pedidos de Reparação';
        else tituloPagina.textContent = 'Portal do Cliente';
    }

    document.querySelectorAll('.menu li').forEach(item => {
        item.classList.toggle('active', item.dataset.section === secao);
    });

    if (secao === 'meus-veiculos') {
        carregarMeusVeiculos();
    } else if (secao === 'dashboard') {
        carregarEstatisticasDashboard();
        carregarPedidosDashboard();
    } else if (secao === 'ordens') {
        carregarMinhasOrdens();
    } else if (secao === 'pecas') {
        carregarPecasDisponiveisCliente();
    } else if (secao === 'pedidos') {
        carregarVeiculosParaPedido();
    }
}

async function carregarEstatisticasDashboard() {
    try {
        const response = await fetch('https://localhost:7085/api/pedidos/meus-pedidos', {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) return;

        const dados = await response.json();
        const pedidos = dados.value || dados;

        if (!Array.isArray(pedidos)) return;

        const totais = pedidos.length;

        const emAnalise = pedidos.filter(p => {
            const st = (p.estado || '').toLowerCase();
            return st === 'pendente' || st === 'em análise' || st === 'em analise';
        }).length;

        const aceitesConcluidos = pedidos.filter(p => {
            const st = (p.estado || '').toLowerCase();
            return st === 'aceite' || st === 'aprovado' || st === 'aprovado - em curso' || st === 'completo' || st === 'concluído' || st === 'concluida';
        }).length;

        const rejeitados = pedidos.filter(p => {
            const st = (p.estado || '').toLowerCase();
            return st === 'rejeitado' || st === 'rejeitada' || st === 'recusado';
        }).length;

        atualizarTextoElemento(['pedidosTotais', 'totalPedidos', 'totalOrdens'], totais);
        atualizarTextoElemento(['emAnalise', 'pedidosEmAnalise', 'emCurso'], emAnalise);
        atualizarTextoElemento(['aceitesConcluidos', 'pedidosAceites', 'concluidas'], aceitesConcluidos);
        atualizarTextoElemento(['pedidosRejeitados', 'rejeitados', 'totalRejeitados', 'rejeitadas'], rejeitados);

    } catch (error) {
        console.error('Erro ao carregar estatísticas do dashboard:', error);
    }
}

async function carregarPedidosDashboard() {
    const tabela = document.getElementById('tabelaDashboardPedidos');

    if (!tabela) return;

    tabela.innerHTML = '';

    try {
        const response = await fetch(
            'https://localhost:7085/api/pedidos/meus-pedidos',
            {
                method: 'GET',
                credentials: 'include'
            }
        );

        if (!response.ok) {
            throw new Error('Não foi possível carregar os pedidos do dashboard.');
        }

        const dados = await response.json();

        pedidosDashboardCarregados = Array.isArray(dados)
            ? dados
            : (dados.value || []);

        const totalPaginas = Math.max(
            1,
            Math.ceil(pedidosDashboardCarregados.length / pedidosDashboardPorPagina)
        );

        if (paginaAtualPedidosDashboard > totalPaginas) {
            paginaAtualPedidosDashboard = totalPaginas;
        }

        renderizarPedidosDashboard();

    } catch (error) {
        console.error(error);
        mostrarTabelaVazia(
            'tabelaDashboardPedidos',
            5,
            'Erro ao carregar pedidos.'
        );
    }
}

function renderizarPedidosDashboard() {
    const tabela = document.getElementById('tabelaDashboardPedidos');

    const totalPaginas = Math.max(
        1,
        Math.ceil(pedidosDashboardCarregados.length / pedidosDashboardPorPagina)
    );

    const inicio = (paginaAtualPedidosDashboard - 1) * pedidosDashboardPorPagina;

    const pedidosDaPagina = pedidosDashboardCarregados.slice(
        inicio,
        inicio + pedidosDashboardPorPagina
    );

    if (pedidosDaPagina.length === 0) {
        mostrarTabelaVazia(
            'tabelaDashboardPedidos',
            5,
            'Ainda não tem nenhum pedido de reparação registado.'
        );
    } else {
        tabela.innerHTML = pedidosDaPagina.map(p => {
            const estadoBruto = p.estado || 'Pendente';

            let estadoExibicao = 'Pendente';
            let estadoClasse = 'curso';

            if (
                estadoBruto === 'Completo' ||
                estadoBruto === 'Concluído' ||
                estadoBruto === 'Concluida'
            ) {
                estadoExibicao = 'Concluído';
                estadoClasse = 'concluida';

            } else if (
                estadoBruto === 'Aceite' ||
                estadoBruto === 'Aprovado' ||
                estadoBruto === 'Aprovado - Em Curso'
            ) {
                estadoExibicao = 'Aprovado - Em Curso';
                estadoClasse = 'concluida';

            } else if (
                estadoBruto === 'Rejeitado' ||
                estadoBruto === 'Recusado'
            ) {
                estadoExibicao = 'Rejeitado';
                estadoClasse = 'erro';
            }

            return `
                <tr>
                    <td>
                        ${p.dataSubmissao
                    ? new Date(p.dataSubmissao).toLocaleDateString()
                    : '-'}
                    </td>
                    <td>Veículo #${p.veiculoId}</td>
                    <td>${escaparHtml(p.descricaoProblema)}</td>
                    <td>
                        <span class="badge ${estadoClasse}">
                            ${escaparHtml(estadoExibicao)}
                        </span>
                    </td>
                    <td>${escaparHtml(p.observacoesAdmin || '-')}</td>
                </tr>
            `;
        }).join('');
    }

    document.getElementById('infoPaginaDashboardPedidos').textContent =
        `Página ${paginaAtualPedidosDashboard} de ${totalPaginas}`;

    document.getElementById('btnAnteriorDashboardPedidos').disabled =
        paginaAtualPedidosDashboard <= 1;

    document.getElementById('btnSeguinteDashboardPedidos').disabled =
        paginaAtualPedidosDashboard >= totalPaginas;
}

function mudarPaginaPedidosDashboard(direcao) {
    const totalPaginas = Math.max(
        1,
        Math.ceil(pedidosDashboardCarregados.length / pedidosDashboardPorPagina)
    );

    const novaPagina = paginaAtualPedidosDashboard + direcao;

    if (novaPagina < 1 || novaPagina > totalPaginas) {
        return;
    }

    paginaAtualPedidosDashboard = novaPagina;
    renderizarPedidosDashboard();
}

function atualizarTextoElemento(idsPossiveis, valor) {
    for (const id of idsPossiveis) {
        const el = document.getElementById(id);
        if (el) {
            el.textContent = valor;
            break;
        }
    }
}

function abrirModalVeiculo() {
    const modal = document.getElementById('modalNovoVeiculo');
    if (modal) modal.style.display = 'flex';
}

function fecharModalVeiculo() {
    const modal = document.getElementById('modalNovoVeiculo');
    if (modal) modal.style.display = 'none';
    document.getElementById('formNovoVeiculo')?.reset();
}

function abrirModalEditarVeiculo(id, matricula, marca, modelo, ano) {
    document.getElementById('editVeiculoId').value = id;
    document.getElementById('editMatriculaInput').value = matricula;
    document.getElementById('editMarcaInput').value = marca;
    document.getElementById('editModeloInput').value = modelo;
    document.getElementById('editAnoInput').value = ano;

    const modal = document.getElementById('modalEditarVeiculo');
    if (modal) modal.style.display = 'flex';
}

function fecharModalEditarVeiculo() {
    const modal = document.getElementById('modalEditarVeiculo');
    if (modal) modal.style.display = 'none';
}

async function carregarMeusVeiculos() {
    const tabela = document.getElementById('tabelaMeusVeiculos');
    if (!tabela) return;
    tabela.innerHTML = '';

    try {
        const response = await fetch('https://localhost:7085/api/Veiculos/meus', {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) throw new Error('Não foi possível carregar veículos.');

        const veiculos = await response.json();

        const totalMeusVeiculos = document.getElementById('totalMeusVeiculos');

        if (totalMeusVeiculos) {
            totalMeusVeiculos.textContent = veiculos.length;
        }

        if (!Array.isArray(veiculos) || veiculos.length === 0) {
            mostrarTabelaVazia('tabelaMeusVeiculos', 6, 'Ainda não tens nenhum veículo registado.');
            return;
        }

        tabela.innerHTML = veiculos.map(v => `
            <tr>
                <td><strong>#${v.id}</strong></td>
                <td>${escaparHtml(v.matricula)}</td>
                <td>${escaparHtml(v.marca)}</td>
                <td>${escaparHtml(v.modelo)}</td>
                <td>${v.ano}</td>
                <td>
                    <button class="btn-action btn-edit"
                            onclick="abrirModalEditarVeiculo(
                                ${v.id},
                                '${escaparHtml(v.matricula)}',
                                '${escaparHtml(v.marca)}',
                                '${escaparHtml(v.modelo)}',
                                ${v.ano}
                            )">
                        <i class="fa-solid fa-pen"></i>
                        Editar
                    </button>

                    <button class="btn-action btn-delete"
                            onclick="eliminarVeiculo(${v.id})">
                        <i class="fa-solid fa-trash"></i>
                        Eliminar
                    </button>
                </td>
            </tr>
        `).join('');
    } catch (error) {
        console.error(error);
        mostrarTabelaVazia('tabelaMeusVeiculos', 6, 'Erro ao carregar veículos.');
    }
}

let aSubmeterVeiculo = false;

async function registarVeiculo(e) {
    e.preventDefault();

    if (aSubmeterVeiculo) return;
    aSubmeterVeiculo = true;

    const btnSubmit = e.target.querySelector('button[type="submit"]');
    if (btnSubmit) btnSubmit.disabled = true;

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
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(novoVeiculo)
        });

        if (response.ok) {
            fecharModalVeiculo();
            carregarMeusVeiculos();
        } else {
            let mensagemErro = 'Erro ao registar o veículo.';
            try {
                const erroData = await response.json();
                if (erroData.message) mensagemErro = erroData.message;
            } catch {
                const textoErro = await response.text();
                if (textoErro) mensagemErro = textoErro;
            }
            alert(mensagemErro);
        }
    } catch (error) {
        console.error(error);
        alert('Erro de ligação ao servidor.');
    } finally {
        aSubmeterVeiculo = false;
        if (btnSubmit) btnSubmit.disabled = false;
    }
}

let aSubmeterEdicao = false;

async function guardarEdicaoVeiculo(e) {
    e.preventDefault();

    if (aSubmeterEdicao) return;
    aSubmeterEdicao = true;

    const btnSubmit = e.target.querySelector('button[type="submit"]');
    if (btnSubmit) btnSubmit.disabled = true;

    const id = document.getElementById('editVeiculoId').value;

    const veiculoAtualizado = {
        id: parseInt(id),
        matricula: document.getElementById('editMatriculaInput').value,
        marca: document.getElementById('editMarcaInput').value,
        modelo: document.getElementById('editModeloInput').value,
        ano: parseInt(document.getElementById('editAnoInput').value)
    };

    try {
        const response = await fetch(`https://localhost:7085/api/Veiculos/${id}`, {
            method: 'PUT',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(veiculoAtualizado)
        });

        if (response.ok) {
            fecharModalEditarVeiculo();
            carregarMeusVeiculos();
        } else {
            let mensagemErro = 'Erro ao atualizar o veículo.';
            try {
                const erroData = await response.json();
                if (erroData.message) mensagemErro = erroData.message;
            } catch {
                const textoErro = await response.text();
                if (textoErro) mensagemErro = textoErro;
            }
            alert(mensagemErro);
        }
    } catch (error) {
        console.error(error);
        alert('Erro de ligação ao servidor.');
    } finally {
        aSubmeterEdicao = false;
        if (btnSubmit) btnSubmit.disabled = false;
    }
}

async function eliminarVeiculo(id) {
    if (!confirm('Tem a certeza que deseja eliminar este veículo?')) return;

    try {
        const response = await fetch(`https://localhost:7085/api/Veiculos/${id}`, {
            method: 'DELETE',
            credentials: 'include'
        });

        if (response.ok) carregarMeusVeiculos();
    } catch (error) {
        console.error(error);
    }
}

let aSubmeterPedido = false;

// Preenche o seletor apenas com os veículos do cliente autenticado.
async function carregarVeiculosParaPedido() {
    const selectVeiculo = document.getElementById('veiculoIdPedido');

    if (!selectVeiculo) return;

    selectVeiculo.disabled = true;
    selectVeiculo.innerHTML = '<option value="">A carregar os teus veículos...</option>';

    try {
        const response = await fetch(
            'https://localhost:7085/api/Veiculos/meus',
            {
                method: 'GET',
                credentials: 'include'
            }
        );

        if (!response.ok) {
            throw new Error('Não foi possível carregar os veículos.');
        }

        const veiculos = await response.json();

        selectVeiculo.innerHTML = '';

        const opcaoInicial = document.createElement('option');
        opcaoInicial.value = '';
        opcaoInicial.textContent = 'Seleciona um veículo';
        selectVeiculo.appendChild(opcaoInicial);

        if (!Array.isArray(veiculos) || veiculos.length === 0) {
            opcaoInicial.textContent = 'Ainda não tens veículos registados';
            return;
        }

        veiculos.forEach(veiculo => {
            const opcao = document.createElement('option');

            opcao.value = veiculo.id;
            opcao.textContent =
                `${veiculo.matricula} — ${veiculo.marca} ${veiculo.modelo} (${veiculo.ano})`;

            selectVeiculo.appendChild(opcao);
        });

        selectVeiculo.disabled = false;

    } catch (error) {
        console.error(error);

        selectVeiculo.innerHTML =
            '<option value="">Erro ao carregar veículos</option>';
    }
}

async function submeterPedido(e) {
    e.preventDefault();

    if (aSubmeterPedido) return;
    aSubmeterPedido = true;

    const btnSubmit = e.target.querySelector('button[type="submit"]');
    if (btnSubmit) btnSubmit.disabled = true;

    const veiculoIdVal = document.getElementById('veiculoIdPedido')?.value;
    const descricaoVal = document.getElementById('descricaoProblema')?.value;

    if (!veiculoIdVal) {
        alert('Seleciona um veículo antes de enviares o pedido.');
        aSubmeterPedido = false;
        if (btnSubmit) btnSubmit.disabled = false;
        return;
    }

    const novoPedido = {
        veiculoId: parseInt(veiculoIdVal),
        descricaoProblema: descricaoVal
    };

    try {
        const response = await fetch('https://localhost:7085/api/pedidos', {
            method: 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(novoPedido)
        });

        if (response.ok) {
            document.getElementById('formNovoPedido')?.reset();
            carregarEstatisticasDashboard();
            carregarPedidosDashboard();
        } else {
            let mensagemErro = 'Erro ao submeter o pedido.';
            try {
                const erroData = await response.json();
                if (erroData.mensagem) mensagemErro = erroData.mensagem;
                else if (erroData.message) mensagemErro = erroData.message;
            } catch {
                const textoErro = await response.text();
                if (textoErro) mensagemErro = textoErro;
            }
            alert(mensagemErro);
        }
    } catch (error) {
        console.error(error);
        alert('Erro de ligação ao servidor.');
    } finally {
        aSubmeterPedido = false;
        if (btnSubmit) btnSubmit.disabled = false;
    }
}

function fecharModalHistoricoPedidos() {
    const modal = document.getElementById('modalHistoricoPedidos');

    if (modal) {
        modal.style.display = 'none';
    }
}

async function abrirModalHistoricoPedidos() {
    const modal = document.getElementById('modalHistoricoPedidos');

    if (!modal) return;

    modal.style.display = 'flex';

    try {
        const response = await fetch(
            'https://localhost:7085/api/pedidos/meus-pedidos',
            {
                method: 'GET',
                credentials: 'include'
            }
        );

        if (!response.ok) {
            throw new Error('Não foi possível carregar o histórico.');
        }

        const dados = await response.json();

        pedidosHistoricoCarregados = Array.isArray(dados)
            ? dados
            : (dados.value || []);

        paginaAtualHistoricoPedidos = 1;

        renderizarHistoricoPedidosModal();

    } catch (error) {
        console.error(error);

        pedidosHistoricoCarregados = [];
        renderizarHistoricoPedidosModal(true);
    }
}

function renderizarHistoricoPedidosModal(temErro = false) {
    const tabela = document.getElementById('tabelaHistoricoPedidosModal');

    if (!tabela) return;

    const totalPaginas = Math.max(
        1,
        Math.ceil(pedidosHistoricoCarregados.length / pedidosPorPaginaHistorico)
    );

    if (paginaAtualHistoricoPedidos > totalPaginas) {
        paginaAtualHistoricoPedidos = totalPaginas;
    }

    const inicio = (paginaAtualHistoricoPedidos - 1) * pedidosPorPaginaHistorico;

    const pedidosDaPagina = pedidosHistoricoCarregados.slice(
        inicio,
        inicio + pedidosPorPaginaHistorico
    );

    if (pedidosDaPagina.length === 0) {
        mostrarTabelaVazia(
            'tabelaHistoricoPedidosModal',
            5,
            temErro
                ? 'Erro ao carregar o histórico de pedidos.'
                : 'Ainda não tens nenhum pedido de reparação registado.'
        );

    } else {
        tabela.innerHTML = pedidosDaPagina.map(pedido => {
            const estadoBruto = pedido.estado || 'Pendente';

            let estadoExibicao = 'Pendente';
            let estadoClasse = 'curso';

            if (
                estadoBruto === 'Completo' ||
                estadoBruto === 'Concluído' ||
                estadoBruto === 'Concluida'
            ) {
                estadoExibicao = 'Concluído';
                estadoClasse = 'concluida';

            } else if (
                estadoBruto === 'Aceite' ||
                estadoBruto === 'Aprovado' ||
                estadoBruto === 'Aprovado - Em Curso'
            ) {
                estadoExibicao = 'Aprovado - Em Curso';
                estadoClasse = 'concluida';

            } else if (
                estadoBruto === 'Rejeitado' ||
                estadoBruto === 'Recusado'
            ) {
                estadoExibicao = 'Rejeitado';
                estadoClasse = 'erro';
            }

            return `
                <tr>
                    <td>
                        ${pedido.dataSubmissao
                    ? new Date(pedido.dataSubmissao).toLocaleDateString()
                    : '-'}
                    </td>
                    <td>Veículo #${pedido.veiculoId}</td>
                    <td>${escaparHtml(pedido.descricaoProblema)}</td>
                    <td>
                        <span class="badge ${estadoClasse}">
                            ${escaparHtml(estadoExibicao)}
                        </span>
                    </td>
                    <td>${escaparHtml(pedido.observacoesAdmin || '-')}</td>
                </tr>
            `;
        }).join('');
    }

    document.getElementById('infoPaginaHistoricoPedidos').textContent =
        `Página ${paginaAtualHistoricoPedidos} de ${totalPaginas}`;

    document.getElementById('btnAnteriorHistoricoPedidos').disabled =
        paginaAtualHistoricoPedidos <= 1;

    document.getElementById('btnSeguinteHistoricoPedidos').disabled =
        paginaAtualHistoricoPedidos >= totalPaginas;
}

function mudarPaginaHistoricoPedidos(direcao) {
    const totalPaginas = Math.max(
        1,
        Math.ceil(pedidosHistoricoCarregados.length / pedidosPorPaginaHistorico)
    );

    const novaPagina = paginaAtualHistoricoPedidos + direcao;

    if (novaPagina < 1 || novaPagina > totalPaginas) {
        return;
    }

    paginaAtualHistoricoPedidos = novaPagina;
    renderizarHistoricoPedidosModal();
}

async function carregarPecasDisponiveisCliente() {
    const tabela = document.getElementById('tabelaPecasDisponiveis');
    if (!tabela) return;
    tabela.innerHTML = '';

    try {
        const response = await fetch('http://localhost:5039/api/Pecas', {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) throw new Error('Não foi possível carregar as peças.');

        const data = await response.json();
        const pecas = data.value || data;

        if (!Array.isArray(pecas) || pecas.length === 0) {
            mostrarTabelaVazia('tabelaPecasDisponiveis', 6, 'Não existem peças disponíveis em stock.');
            return;
        }

        tabela.innerHTML = pecas.map(p => `
            <tr>
                <td><strong>${escaparHtml(p.referenciaPeca || p.id)}</strong></td>
                <td>${escaparHtml(p.nome)}</td>
                <td>${escaparHtml(p.categoria || '-')}</td>
                <td>${escaparHtml(p.compatibilidade || 'Geral')}</td>
                <td><strong>${Number(p.precoUnitario || 0).toFixed(2)} €</strong></td>
                <td>${p.stockDisponivel ?? 0}</td>
            </tr>
        `).join('');
    } catch (error) {
        console.error(error);
        mostrarTabelaVazia('tabelaPecasDisponiveis', 6, 'Erro ao carregar peças.');
    }
}

async function carregarMinhasOrdens(pagina = 1) {
    const tabela = document.getElementById('tabelaOrdens');
    if (!tabela) return;

    const veiculoIdFiltro = document.getElementById('filtroVeiculoId')?.value || '';
    let url = `https://localhost:7085/api/OrdensReparacao/minhas-ordens?pagina=${pagina}`;
    if (veiculoIdFiltro) url += `&veiculoId=${veiculoIdFiltro}`;

    try {
        const response = await fetch(url, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            mostrarTabelaVazia('tabelaOrdens', 6, 'Não foi possível carregar as reparações.');
            return;
        }

        const dados = await response.json();
        const ordens = dados.itens || dados;

        if (!Array.isArray(ordens) || ordens.length === 0) {
            mostrarTabelaVazia('tabelaOrdens', 6, 'Não tem nenhuma ordem de reparação registada.');
            return;
        }

        tabela.innerHTML = ordens.map(o => {
            const estado = o.estado || 'Em Curso';
            const classeBadge = estado === 'Concluída' ? 'concluida' : 'curso';

            return `
                <tr>
                    <td><strong>#${o.id}</strong></td>
                    <td>Veículo #${o.veiculoId}</td>
                    <td>${escaparHtml(o.descricaoProblema)}</td>
                    <td><strong>${Number(o.valorTotal || 0).toFixed(2)} €</strong></td>
                    <td>
                        <span class="badge ${classeBadge}">
                            <i class="fa-solid fa-circle" style="font-size: 6px;"></i> ${estado}
                        </span>
                    </td>
                    <td>
                        <button class="btn-action" onclick="verDetalhesOrdemCliente(${o.id})">
                            <i class="fa-solid fa-eye"></i> Detalhes
                        </button>
                    </td>
                </tr>
            `;
        }).join('');

        if (dados.totalPaginas) {
            paginaAtualCliente = pagina;
            const infoPagina = document.getElementById('infoPagina');
            if (infoPagina) infoPagina.textContent = `Página ${paginaAtualCliente} de ${dados.totalPaginas}`;

            const btnAnt = document.getElementById('btnPaginaAnterior');
            if (btnAnt) btnAnt.disabled = paginaAtualCliente <= 1;

            const btnSeg = document.getElementById('btnPaginaSeguinte');
            if (btnSeg) btnSeg.disabled = paginaAtualCliente >= dados.totalPaginas;
        }

    } catch (error) {
        console.error(error);
        mostrarTabelaVazia('tabelaOrdens', 6, 'Erro ao carregar reparações.');
    }
}

function aplicarFiltroVeiculo() {
    carregarMinhasOrdens(1);
}

function limparFiltroVeiculo() {
    const input = document.getElementById('filtroVeiculoId');
    if (input) input.value = '';
    carregarMinhasOrdens(1);
}

function mudarPagina(direcao) {
    const novaPagina = paginaAtualCliente + direcao;
    if (novaPagina > 0) {
        carregarMinhasOrdens(novaPagina);
    }
}

async function verDetalhesOrdemCliente(id) {
    try {
        const response = await fetch(`https://localhost:7085/api/OrdensReparacao/${id}`, {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) throw new Error('Erro ao carregar detalhes.');

        const o = await response.json();

        document.getElementById('detalheVeiculo').textContent = `Veículo #${o.veiculoId}`;
        document.getElementById('detalheDataEntrada').textContent = o.dataEntrada ? new Date(o.dataEntrada).toLocaleDateString() : '-';
        document.getElementById('detalheDataConclusao').textContent = o.dataConclusao ? new Date(o.dataConclusao).toLocaleDateString() : 'A decorrer';
        document.getElementById('detalheMaoDeObra').textContent = `${Number(o.custoMaoDeObra || 0).toFixed(2)} €`;
        document.getElementById('detalheCustoPecas').textContent = `${Number(o.custoPecas || 0).toFixed(2)} €`;
        document.getElementById('detalheTotal').textContent = `${Number(o.valorTotal || 0).toFixed(2)} €`;
        document.getElementById('detalheDescricao').textContent = o.descricaoProblema || 'Sem descrição';

        const listaPecas = document.getElementById('listaPecasDetalhe');
        if (o.pecasAplicadas && o.pecasAplicadas.length > 0) {
            listaPecas.innerHTML = o.pecasAplicadas.map(p => `<div>- ${escaparHtml(p.nome)} (${p.quantidade}x)</div>`).join('');
        } else {
            listaPecas.innerHTML = '<em>Nenhuma peça registada.</em>';
        }

        document.getElementById('modalDetalhesOrdem').style.display = 'flex';
    } catch (error) {
        console.error(error);
        alert('Não foi possível carregar os detalhes da ordem.');
    }
}

function fecharModalDetalhes() {
    document.getElementById('modalDetalhesOrdem').style.display = 'none';
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
                <td colspan="${numeroColunas}" style="text-align: center; padding: 20px;">
                    ${escaparHtml(mensagem)}
                </td>
            </tr>`;
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
            const welcomeMsg = document.getElementById('welcomeMessage');
            if (welcomeMsg) welcomeMsg.textContent = `Bem-vindo, ${data.firstName}!`;
        }
    } catch (error) {
        console.error(error);
    }
}

function terminarSessao() {
    window.location.href = 'https://localhost:7194/login.html';
}

document.addEventListener('DOMContentLoaded', () => {
    carregarNomeUtilizador();
    mostrarSecaoCliente('dashboard');

    const formVeiculo = document.getElementById('formNovoVeiculo');
    if (formVeiculo) formVeiculo.addEventListener('submit', registarVeiculo);

    const formEditar = document.getElementById('formEditarVeiculo');
    if (formEditar) formEditar.addEventListener('submit', guardarEdicaoVeiculo);

    const formPedido = document.getElementById('formNovoPedido');
    if (formPedido) formPedido.addEventListener('submit', submeterPedido);
});
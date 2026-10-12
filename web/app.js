'use strict';

const API_BASE = 'https://api.coingecko.com/api/v3';
const CACHE_KEY = 'crypto-market-v1';
const CACHE_TTL = 90_000;
const MAX_COINS = 100;
const fiatLocales = { usd: 'en-US', eur: 'de-DE', gbp: 'en-GB', uah: 'uk-UA' };

const state = {
  coins: [],
  sort: 'rank',
  direction: 1,
  loading: false
};

const marketBody = document.querySelector('#market-body');
const marketState = document.querySelector('#market-state');
const searchInput = document.querySelector('#search-input');
const resultCount = document.querySelector('#result-count');
const coinSelect = document.querySelector('#coin-select');
const fiatSelect = document.querySelector('#fiat-select');
const amountInput = document.querySelector('#amount-input');
const converterForm = document.querySelector('#converter-form');
const converterError = document.querySelector('#converter-error');
const conversionResult = document.querySelector('#conversion-result');
const conversionRate = document.querySelector('#conversion-rate');
const coinDialog = document.querySelector('#coin-dialog');
const chartLine = document.querySelector('#chart-line');
const chartLoading = document.querySelector('#chart-loading');

function safeCoinId(value) {
  return typeof value === 'string' && /^[a-z0-9-]{1,100}$/.test(value);
}

async function fetchJson(path, timeout = 15_000) {
  const controller = new AbortController();
  const timer = window.setTimeout(() => controller.abort(), timeout);
  try {
    const response = await fetch(`${API_BASE}${path}`, {
      headers: { Accept: 'application/json' },
      signal: controller.signal,
      cache: 'no-store'
    });
    if (!response.ok) throw new Error(`Сервис ответил с кодом ${response.status}`);
    const contentType = response.headers.get('content-type') || '';
    if (!contentType.includes('application/json')) throw new Error('Получен ответ неизвестного формата');
    return await response.json();
  } catch (error) {
    if (error.name === 'AbortError') throw new Error('Сервис не ответил вовремя');
    throw error;
  } finally {
    window.clearTimeout(timer);
  }
}

function formatMoney(value, currency = 'USD', compact = false) {
  if (!Number.isFinite(value)) return '—';
  const maximumFractionDigits = value > 0 && value < 1 ? 8 : 2;
  return new Intl.NumberFormat('ru-RU', {
    style: 'currency',
    currency,
    notation: compact ? 'compact' : 'standard',
    maximumFractionDigits: compact ? 2 : maximumFractionDigits
  }).format(value);
}

function formatPercent(value) {
  if (!Number.isFinite(value)) return '—';
  return `${value > 0 ? '+' : ''}${value.toFixed(2)}%`;
}

function setTrend(element, value) {
  element.classList.remove('positive', 'negative');
  if (Number.isFinite(value)) element.classList.add(value >= 0 ? 'positive' : 'negative');
}

function validatedImageUrl(value) {
  try {
    const url = new URL(value);
    return url.protocol === 'https:' && url.hostname === 'coin-images.coingecko.com' ? url.href : '';
  } catch {
    return '';
  }
}

function createCell(className = '') {
  const cell = document.createElement('td');
  if (className) cell.className = className;
  return cell;
}

function renderMarket() {
  const query = searchInput.value.trim().toLocaleLowerCase('ru');
  const filtered = state.coins.filter((coin) => {
    const name = String(coin.name || '').toLocaleLowerCase('ru');
    const symbol = String(coin.symbol || '').toLocaleLowerCase('ru');
    return !query || name.includes(query) || symbol.includes(query);
  });

  const sortValues = {
    rank: (coin) => coin.market_cap_rank ?? Number.MAX_SAFE_INTEGER,
    price: (coin) => coin.current_price ?? 0,
    change: (coin) => coin.price_change_percentage_24h ?? -Infinity,
    volume: (coin) => coin.total_volume ?? 0,
    cap: (coin) => coin.market_cap ?? 0
  };
  filtered.sort((a, b) => (sortValues[state.sort](a) - sortValues[state.sort](b)) * state.direction);

  const fragment = document.createDocumentFragment();
  filtered.forEach((coin) => {
    const row = document.createElement('tr');
    row.tabIndex = 0;
    row.setAttribute('aria-label', `Открыть данные ${coin.name}`);
    row.addEventListener('click', () => openCoin(coin));
    row.addEventListener('keydown', (event) => {
      if (event.key === 'Enter' || event.key === ' ') {
        event.preventDefault();
        openCoin(coin);
      }
    });

    const rank = createCell('rank-column');
    rank.textContent = coin.market_cap_rank ?? '—';

    const asset = createCell();
    const assetWrap = document.createElement('div');
    assetWrap.className = 'coin-cell';
    const image = document.createElement('img');
    image.src = validatedImageUrl(coin.image);
    image.alt = '';
    image.loading = 'lazy';
    const names = document.createElement('span');
    names.className = 'coin-name';
    const title = document.createElement('span');
    title.textContent = coin.name || 'Без названия';
    const symbol = document.createElement('small');
    symbol.textContent = coin.symbol || '—';
    names.append(title, symbol);
    assetWrap.append(image, names);
    asset.append(assetWrap);

    const price = createCell('number-column');
    price.textContent = formatMoney(coin.current_price);
    const change = createCell('number-column');
    change.textContent = formatPercent(coin.price_change_percentage_24h);
    setTrend(change, coin.price_change_percentage_24h);
    const volume = createCell('number-column optional-column');
    volume.textContent = formatMoney(coin.total_volume, 'USD', true);
    const cap = createCell('number-column optional-column');
    cap.textContent = formatMoney(coin.market_cap, 'USD', true);
    row.append(rank, asset, price, change, volume, cap);
    fragment.append(row);
  });

  marketBody.replaceChildren(fragment);
  resultCount.textContent = filtered.length ? `Показано ${filtered.length} из ${state.coins.length}` : 'Совпадений нет';
  if (!filtered.length && state.coins.length) showMarketMessage('Ничего не найдено. Попробуйте другое название или тикер.', false);
  else marketState.classList.add('hidden');
}

function populateCoinSelect() {
  const fragment = document.createDocumentFragment();
  state.coins.forEach((coin) => {
    const option = document.createElement('option');
    option.value = safeCoinId(coin.id) ? coin.id : '';
    option.textContent = `${coin.name} (${String(coin.symbol || '').toUpperCase()})`;
    if (coin.id === 'bitcoin') option.selected = true;
    fragment.append(option);
  });
  coinSelect.replaceChildren(fragment);
}

function updateGlobalMetrics(globalData) {
  const data = globalData?.data;
  if (!data) return;
  document.querySelector('#global-cap').textContent = formatMoney(data.total_market_cap?.usd, 'USD', true);
  document.querySelector('#global-volume').textContent = formatMoney(data.total_volume?.usd, 'USD', true);
  document.querySelector('#btc-dominance').textContent = Number.isFinite(data.market_cap_percentage?.btc)
    ? `${data.market_cap_percentage.btc.toFixed(1)}%`
    : '—';
  const change = document.querySelector('#global-change');
  change.textContent = `${formatPercent(data.market_cap_change_percentage_24h_usd)} за 24 часа`;
  setTrend(change, data.market_cap_change_percentage_24h_usd);
}

function showMarketMessage(message, isError = true) {
  marketState.classList.remove('hidden');
  marketState.classList.toggle('error', isError);
  marketState.replaceChildren();
  const text = document.createElement('p');
  text.textContent = message;
  marketState.append(text);
  if (isError) {
    const retry = document.createElement('button');
    retry.type = 'button';
    retry.className = 'button button-primary';
    retry.textContent = 'Повторить';
    retry.addEventListener('click', () => loadMarket(true));
    marketState.append(retry);
  }
}

function setLoading() {
  marketState.classList.remove('hidden', 'error');
  marketState.replaceChildren();
  const spinner = document.createElement('div');
  spinner.className = 'spinner';
  spinner.setAttribute('aria-hidden', 'true');
  const text = document.createElement('p');
  text.textContent = 'Получаем свежие данные рынка…';
  marketState.append(spinner, text);
}

function readCache() {
  try {
    const cached = JSON.parse(sessionStorage.getItem(CACHE_KEY));
    return cached && Date.now() - cached.time < CACHE_TTL ? cached : null;
  } catch {
    return null;
  }
}

async function loadMarket(force = false) {
  if (state.loading) return;
  state.loading = true;
  document.querySelector('#refresh-button').disabled = true;
  setLoading();
  try {
    const cached = force ? null : readCache();
    let coins;
    let globalData;
    if (cached) {
      ({ coins, globalData } = cached);
    } else {
      [coins, globalData] = await Promise.all([
        fetchJson(`/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=${MAX_COINS}&page=1&sparkline=false&price_change_percentage=24h`),
        fetchJson('/global')
      ]);
      sessionStorage.setItem(CACHE_KEY, JSON.stringify({ time: Date.now(), coins, globalData }));
    }
    if (!Array.isArray(coins) || !coins.length) throw new Error('Список активов пуст');
    state.coins = coins.filter((coin) => safeCoinId(coin.id));
    updateGlobalMetrics(globalData);
    populateCoinSelect();
    renderMarket();
    document.querySelector('#updated-time').textContent = `Обновлено: ${new Intl.DateTimeFormat('ru-RU', { hour: '2-digit', minute: '2-digit' }).format(new Date())}`;
  } catch (error) {
    showMarketMessage(`Не удалось загрузить рынок. ${error.message}. Проверьте подключение и попробуйте снова.`);
    resultCount.textContent = 'Данные недоступны';
  } finally {
    state.loading = false;
    document.querySelector('#refresh-button').disabled = false;
  }
}

async function openCoin(coin) {
  if (!safeCoinId(coin.id)) return;
  document.querySelector('#dialog-title').textContent = coin.name;
  document.querySelector('#dialog-symbol').textContent = String(coin.symbol || '').toUpperCase();
  document.querySelector('#dialog-image').src = validatedImageUrl(coin.image);
  document.querySelector('#dialog-price').textContent = formatMoney(coin.current_price);
  const change = document.querySelector('#dialog-change');
  change.textContent = formatPercent(coin.price_change_percentage_24h);
  setTrend(change, coin.price_change_percentage_24h);
  document.querySelector('#dialog-cap').textContent = formatMoney(coin.market_cap, 'USD', true);
  document.querySelector('#dialog-volume').textContent = formatMoney(coin.total_volume, 'USD', true);
  document.querySelector('#dialog-rank').textContent = coin.market_cap_rank ? `№ ${coin.market_cap_rank}` : '—';
  chartLine.setAttribute('points', '');
  chartLoading.classList.remove('hidden');
  chartLoading.replaceChildren();
  const spinner = document.createElement('div');
  spinner.className = 'spinner';
  const message = document.createElement('span');
  message.textContent = 'Строим график…';
  chartLoading.append(spinner, message);
  if (!coinDialog.open) coinDialog.showModal();

  try {
    const history = await fetchJson(`/coins/${encodeURIComponent(coin.id)}/market_chart?vs_currency=usd&days=1`);
    const prices = Array.isArray(history.prices)
      ? history.prices.filter((point) => Array.isArray(point) && point.length >= 2 && Number.isFinite(point[1]))
      : [];
    if (prices.length < 2) throw new Error('Недостаточно данных для графика');
    const values = prices.map((point) => point[1]);
    const min = Math.min(...values);
    const max = Math.max(...values);
    const range = Math.max(max - min, max * .001, Number.EPSILON);
    const points = prices.map((point, index) => {
      const x = 760 * index / (prices.length - 1);
      const y = 265 - ((point[1] - min) / range * 245);
      return `${x.toFixed(1)},${y.toFixed(1)}`;
    }).join(' ');
    chartLine.setAttribute('points', points);
    chartLoading.classList.add('hidden');
  } catch (error) {
    chartLoading.replaceChildren();
    const message = document.createElement('span');
    message.textContent = `График недоступен: ${error.message}`;
    chartLoading.append(message);
  }
}

async function convertCurrency(event) {
  event.preventDefault();
  converterError.textContent = '';
  const amount = Number(amountInput.value.trim().replace(/\s/g, '').replace(',', '.'));
  const coinId = coinSelect.value;
  const fiat = fiatSelect.value;
  if (!Number.isFinite(amount) || amount <= 0 || amount > 1e15) {
    converterError.textContent = 'Введите положительное число разумного размера.';
    return;
  }
  if (!safeCoinId(coinId) || !/^[a-z]{3}$/.test(fiat)) {
    converterError.textContent = 'Выберите обе валюты.';
    return;
  }

  const button = document.querySelector('#convert-button');
  button.disabled = true;
  button.textContent = 'Считаем…';
  try {
    const data = await fetchJson(`/simple/price?ids=${encodeURIComponent(coinId)}&vs_currencies=${encodeURIComponent(fiat)}`);
    const rate = data?.[coinId]?.[fiat];
    if (!Number.isFinite(rate)) throw new Error('Курс для этой пары недоступен');
    const currency = fiat.toUpperCase();
    conversionResult.textContent = new Intl.NumberFormat(fiatLocales[fiat] || 'ru-RU', {
      style: 'currency', currency, maximumFractionDigits: 8
    }).format(amount * rate);
    const coin = state.coins.find((item) => item.id === coinId);
    conversionRate.textContent = `1 ${String(coin?.symbol || coinId).toUpperCase()} = ${formatMoney(rate, currency)}`;
  } catch (error) {
    converterError.textContent = `${error.message}. Попробуйте ещё раз.`;
  } finally {
    button.disabled = false;
    button.textContent = 'Рассчитать';
  }
}

searchInput.addEventListener('input', renderMarket);
document.querySelector('#refresh-button').addEventListener('click', () => loadMarket(true));
converterForm.addEventListener('submit', convertCurrency);
document.querySelector('#dialog-close').addEventListener('click', () => coinDialog.close());
coinDialog.addEventListener('click', (event) => {
  if (event.target === coinDialog) coinDialog.close();
});
document.querySelectorAll('.sort-button').forEach((button) => {
  button.addEventListener('click', () => {
    const nextSort = button.dataset.sort;
    if (state.sort === nextSort) state.direction *= -1;
    else {
      state.sort = nextSort;
      state.direction = nextSort === 'rank' ? 1 : -1;
    }
    document.querySelectorAll('.sort-button').forEach((item) => item.classList.toggle('active', item === button));
    renderMarket();
  });
});

loadMarket();

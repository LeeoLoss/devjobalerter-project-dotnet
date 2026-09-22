import { useEffect, useMemo, useState } from 'react'
import './App.css'

const API_URL = import.meta.env.VITE_API_URL || '/api/jobs'
const filterOptions = ['Remoto', 'Híbrido', 'Júnior', 'Pleno', 'Sênior']

function Icon({ name, size = 20 }) {
  const paths = {
    bell: <><path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9" /><path d="M10 21h4" /></>,
    search: <><circle cx="11" cy="11" r="6" /><path d="m20 20-4.2-4.2" /></>,
    pin: <><path d="M20 10c0 5-8 11-8 11S4 15 4 10a8 8 0 1 1 16 0Z" /><circle cx="12" cy="10" r="2.5" /></>,
    clock: <><circle cx="12" cy="12" r="9" /><path d="M12 7v5l3 2" /></>,
    arrow: <><path d="M5 12h14" /><path d="m13 6 6 6-6 6" /></>,
    filter: <><path d="M4 6h16M7 12h10M10 18h4" /></>,
    refresh: <><path d="M20 11a8 8 0 1 0 2 5" /><path d="M20 4v7h-7" /></>,
    briefcase: <><rect x="3" y="7" width="18" height="12" rx="2" /><path d="M8 7V5a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2M3 12h18" /></>,
    settings: <><circle cx="12" cy="12" r="3" /><path d="M19.4 15a1.7 1.7 0 0 0 .34 1.88l.06.06-2.12 2.12-.06-.06a1.7 1.7 0 0 0-1.88-.34 1.7 1.7 0 0 0-1.04 1.56V20h-3v-.08A1.7 1.7 0 0 0 10.66 18.36a1.7 1.7 0 0 0-1.88.34l-.06.06-2.12-2.12.06-.06A1.7 1.7 0 0 0 7 14.7a1.7 1.7 0 0 0-1.56-1.04H5v-3h.08A1.7 1.7 0 0 0 6.64 9.62 1.7 1.7 0 0 0 6.3 7.74l-.06-.06 2.12-2.12.06.06A1.7 1.7 0 0 0 10.3 6a1.7 1.7 0 0 0 1.04-1.56V4h3v.08A1.7 1.7 0 0 0 15.38 5.64a1.7 1.7 0 0 0 1.88-.34l.06-.06 2.12 2.12-.06.06A1.7 1.7 0 0 0 19 9.3a1.7 1.7 0 0 0 1.56 1.04H21v3h-.08A1.7 1.7 0 0 0 19.4 15Z" /></>,
    check: <><path d="m5 12 4 4L19 6" /></>,
  }
  return <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">{paths[name]}</svg>
}

function formatDate(value) {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Recentemente'
  const minutes = Math.max(0, Math.round((Date.now() - date.getTime()) / 60000))
  if (minutes < 60) return `Há ${Math.max(1, minutes)} min`
  if (minutes < 1440) return `Há ${Math.round(minutes / 60)} h`
  return date.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' })
}

function normalizeJobs(payload) {
  const list = Array.isArray(payload) ? payload : payload?.jobs || payload?.items || payload?.data
  if (!Array.isArray(list)) return []
  return list.map((job, index) => ({
    id: job.id || job.url || job.jobUrl || index,
    title: job.title || 'Vaga de desenvolvimento',
    company: job.company || 'Empresa não informada',
    location: job.location || 'Localização não informada',
    dateFound: formatDate(job.dateFound || job.sentAt),
    url: job.url || job.jobUrl,
    tags: Array.isArray(job.tags) && job.tags.length ? job.tags : ['Nova oportunidade'],
    accent: ['purple', 'blue', 'orange', 'green'][index % 4],
  }))
}

function App() {
  const [jobs, setJobs] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [query, setQuery] = useState('')
  const [filterOpen, setFilterOpen] = useState(false)
  const [activeFilters, setActiveFilters] = useState([])
  const [page, setPage] = useState(() => window.location.hash.slice(1) || 'vagas')
  const [apiOnline, setApiOnline] = useState(false)
  const [saved, setSaved] = useState(false)

  const navigate = (nextPage) => {
    window.location.hash = nextPage
    setPage(nextPage)
  }

  const loadJobs = async () => {
    setLoading(true)
    setError('')
    try {
      const [jobsResponse, healthResponse] = await Promise.all([fetch(API_URL), fetch('/api/health')])
      if (!jobsResponse.ok) throw new Error('jobs')
      setJobs(normalizeJobs(await jobsResponse.json()))
      setApiOnline(healthResponse.ok)
    } catch {
      setJobs([])
      setApiOnline(false)
      setError('A API não está disponível. Inicie o backend para consultar as vagas.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    const requestTimer = window.setTimeout(loadJobs, 0)
    const handleHash = () => setPage(window.location.hash.slice(1) || 'vagas')
    window.addEventListener('hashchange', handleHash)
    return () => { window.clearTimeout(requestTimer); window.removeEventListener('hashchange', handleHash) }
  }, [])

  const visibleJobs = useMemo(() => {
    const term = query.trim().toLocaleLowerCase('pt-BR')
    return jobs.filter((job) => {
      const content = `${job.title} ${job.company} ${job.location} ${job.tags.join(' ')}`.toLocaleLowerCase('pt-BR')
      const matchesQuery = !term || content.includes(term)
      const matchesFilters = activeFilters.every((filter) => content.includes(filter.toLocaleLowerCase('pt-BR')))
      return matchesQuery && matchesFilters
    })
  }, [jobs, query, activeFilters])

  const toggleFilter = (filter) => setActiveFilters((current) => current.includes(filter) ? current.filter((item) => item !== filter) : [...current, filter])
  const pageTitle = { vagas: 'Olá, Leonardo', alertas: 'Seus alertas', configuracoes: 'Configurações' }[page] || 'Olá, Leonardo'

  return <main className="app-shell">
    <aside className="sidebar">
      <a className="brand" href="#vagas" onClick={() => navigate('vagas')}><span className="brand-mark"><Icon name="briefcase" size={19} /></span><span>devjob<span>alert</span></span></a>
      <nav aria-label="Navegação principal">
        <button className={`nav-link ${page === 'vagas' ? 'active' : ''}`} onClick={() => navigate('vagas')}><Icon name="briefcase" />Vagas <span className="nav-count">{jobs.length}</span></button>
        <button className={`nav-link ${page === 'alertas' ? 'active' : ''}`} onClick={() => navigate('alertas')}><Icon name="bell" />Alertas</button>
        <button className={`nav-link ${page === 'configuracoes' ? 'active' : ''}`} onClick={() => navigate('configuracoes')}><Icon name="settings" />Configurações</button>
      </nav>
      <div className="sidebar-footer"><div className="help-card"><span className="help-icon">✦</span><strong>Novas vagas, sem esforço</strong><p>Receba oportunidades no WhatsApp assim que aparecerem.</p><button onClick={() => navigate('alertas')}>Gerenciar alertas <Icon name="arrow" size={15} /></button></div><button className="profile" onClick={() => navigate('configuracoes')}><span className="avatar">LL</span><span><strong>Leonardo Lima</strong><small>Plano gratuito</small></span><span className="dots">···</span></button></div>
    </aside>
    <section className="content" id="top">
      <header className="topbar"><button className="mobile-brand" onClick={() => navigate('vagas')}><span className="brand-mark"><Icon name="briefcase" size={17} /></span>devjob<span>alert</span></button><div className="status"><span className={`status-dot ${apiOnline ? '' : 'offline'}`} />{apiOnline ? 'Alertas ativos' : 'API offline'}</div><button className="notification" aria-label="Abrir alertas" onClick={() => navigate('alertas')}><Icon name="bell" /><span /></button></header>
      <div className="page-header"><div><p className="eyebrow">{page === 'vagas' ? 'PAINEL DE OPORTUNIDADES' : 'DEVJOB ALERT'}</p><h1>{pageTitle}{page === 'vagas' && <span> 👋</span>}</h1><p className="subtitle">{page === 'vagas' ? 'Estas são as vagas mais recentes para você.' : page === 'alertas' ? 'Acompanhe as notificações enviadas pelo monitor.' : 'Controle como o monitor consulta suas oportunidades.'}</p></div>{page === 'vagas' && <button className="refresh-button" onClick={loadJobs} disabled={loading}><Icon name="refresh" size={18} />{loading ? 'Atualizando...' : 'Atualizar'}</button>}</div>
      {error && <div className="notice" role="status">{error}</div>}
      {page === 'vagas' && <JobsPage jobs={jobs} visibleJobs={visibleJobs} query={query} setQuery={setQuery} filterOpen={filterOpen} setFilterOpen={setFilterOpen} activeFilters={activeFilters} toggleFilter={toggleFilter} clearFilters={() => setActiveFilters([])} />}
      {page === 'alertas' && <AlertsPage jobs={jobs} onNavigate={() => navigate('configuracoes')} />}
      {page === 'configuracoes' && <SettingsPage apiOnline={apiOnline} saved={saved} onSave={() => { setSaved(true); window.setTimeout(() => setSaved(false), 2500) }} />}
    </section>
  </main>
}

function JobsPage({ jobs, visibleJobs, query, setQuery, filterOpen, setFilterOpen, activeFilters, toggleFilter, clearFilters }) {
  const hasSearchCriteria = Boolean(query.trim() || activeFilters.length)
  return <><section className="summary-grid"><article><div className="summary-icon lilac"><Icon name="briefcase" /></div><div><strong>{jobs.length}</strong><span>Vagas encontradas</span></div><small>Últimas 24h</small></article><article><div className="summary-icon peach"><Icon name="bell" /></div><div><strong>{jobs.length}</strong><span>Alertas enviados</span></div><small>Este mês</small></article><article><div className="summary-icon mint"><Icon name="clock" /></div><div><strong>12 min</strong><span>Última atualização</span></div><small className="live">● Ao vivo</small></article></section><section className="jobs-section"><div className="section-heading"><div><h2>Vagas recomendadas</h2><p>{hasSearchCriteria ? `${visibleJobs.length} resultado(s) para os filtros aplicados.` : `${visibleJobs.length} oportunidade(s) carregada(s) pela API.`}</p></div><button className={`filter-button ${filterOpen ? 'selected' : ''}`} onClick={() => setFilterOpen(!filterOpen)}><Icon name="filter" size={17} />Filtros{activeFilters.length ? ` (${activeFilters.length})` : ''}</button></div><div className="search-row"><label className="search"><Icon name="search" size={20} /><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Buscar por cargo, empresa ou tecnologia" /></label>{filterOpen && <div className="filter-panel"><div>{filterOptions.map((filter) => <button className={activeFilters.includes(filter) ? 'selected' : ''} aria-pressed={activeFilters.includes(filter)} onClick={() => toggleFilter(filter)} key={filter}>{activeFilters.includes(filter) && <Icon name="check" size={13} />}{filter}</button>)}</div>{activeFilters.length > 0 && <button className="clear-filters" onClick={clearFilters}>Limpar filtros</button>}</div>}</div><div className="jobs-list">{visibleJobs.map((job) => <article className="job-card" key={job.id}><div className={`company-logo ${job.accent}`}>{job.company.slice(0, 1)}</div><div className="job-main"><div className="job-title-row"><div><h3>{job.title}</h3><p>{job.company}</p></div><span className="new-badge">NOVA</span></div><div className="job-meta"><span><Icon name="pin" size={15} />{job.location}</span><span><Icon name="clock" size={15} />{job.dateFound}</span></div><div className="tags">{job.tags.map((tag) => <span key={tag}>{tag}</span>)}</div></div><a className="details-link" href={job.url || '#vagas'} target={job.url ? '_blank' : undefined} rel="noreferrer">Ver vaga <Icon name="arrow" size={16} /></a></article>)}{!visibleJobs.length && <div className="empty-state"><Icon name="search" size={28} /><strong>{hasSearchCriteria ? 'Nenhuma vaga corresponde à busca' : 'Nenhuma vaga encontrada'}</strong><p>{hasSearchCriteria ? 'Revise as palavras-chave ou limpe os filtros.' : 'Ainda não há vagas enviadas pelo monitor.'}</p></div>}</div></section></>
}

function AlertsPage({ jobs, onNavigate }) { return <section className="page-panel"><div className="panel-heading"><div className="summary-icon peach"><Icon name="bell" /></div><div><h2>Histórico de alertas</h2><p>Cada vaga enviada pelo Worker é registrada aqui.</p></div></div>{jobs.length ? <div className="activity-list">{jobs.map((job) => <div className="activity" key={job.id}><span className="activity-check"><Icon name="check" size={14} /></span><div><strong>Alerta enviado: {job.title}</strong><p>{job.company} · {job.dateFound}</p></div></div>)}</div> : <div className="empty-state"><Icon name="bell" size={28} /><strong>Nenhum alerta enviado ainda</strong><p>Assim que o Worker encontrar uma vaga, ela aparecerá neste histórico.</p><button className="primary-button" onClick={onNavigate}>Configurar monitor</button></div>}</section> }

function SettingsPage({ apiOnline, saved, onSave }) {
  const [keywords, setKeywords] = useState(() => window.localStorage.getItem('devjob-keywords') || '')
  const [location, setLocation] = useState(() => window.localStorage.getItem('devjob-location') || '')
  const savePreferences = (event) => { event.preventDefault(); window.localStorage.setItem('devjob-keywords', keywords); window.localStorage.setItem('devjob-location', location); onSave() }
  return <section className="page-panel"><div className="panel-heading"><div className="summary-icon lilac"><Icon name="settings" /></div><div><h2>Monitor de oportunidades</h2><p>Confira a conexão e defina suas preferências de acompanhamento.</p></div></div><div className="settings-grid"><article className="setting-card"><span className={`connection-dot ${apiOnline ? '' : 'offline'}`} /> <div><strong>{apiOnline ? 'API conectada' : 'API indisponível'}</strong><p>{apiOnline ? 'O painel está recebendo dados do backend.' : 'Inicie a API .NET na porta 5243.'}</p></div></article><article className="setting-card"><Icon name="bell" /><div><strong>Canal de alerta</strong><p>WhatsApp é acionado pelo Worker após a configuração das credenciais.</p></div></article></div><form className="preferences" onSubmit={savePreferences}><h3>Preferências de exibição</h3><label>Palavras-chave favoritas<input value={keywords} onChange={(event) => setKeywords(event.target.value)} placeholder="Ex.: React, .NET, remoto" /></label><label>Localização preferida<input value={location} onChange={(event) => setLocation(event.target.value)} placeholder="Ex.: Brasil" /></label><button className="primary-button" type="submit">{saved ? 'Preferências salvas' : 'Salvar preferências'}</button></form></section>
}

export default App

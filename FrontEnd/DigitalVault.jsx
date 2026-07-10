import { createContext, useContext, useState, useCallback } from "react";


// ─── DATA ───────────────────────────────────────────────────────────────────
const categories = [
  { CategoryId: 1, Name: "全部", Slug: "all" },
  { CategoryId: 2, Name: "AI 提示詞", Slug: "ai-prompts" },
  { CategoryId: 3, Name: "UI 套件", Slug: "ui-kits" },
  { CategoryId: 4, Name: "模板", Slug: "templates" },
  { CategoryId: 5, Name: "電子書", Slug: "ebooks" },
  { CategoryId: 6, Name: "程式資源", Slug: "code" },
];

const products = [
  { ProductId: 1, CategoryId: 2, Name: "AI Prompt Pack Pro", Description: "200 組精選 ChatGPT / Midjourney 提示詞，涵蓋行銷、設計、程式等場景。", Price: 12, ThumbnailUrl: "https://picsum.photos/400/220?1", IsPublished: true, avgRating: 4.9, reviewCount: 128 },
  { ProductId: 2, CategoryId: 3, Name: "Dark UI Kit", Description: "50+ 深色主題元件，含 Figma 原始檔、標註說明與 React 程式碼範本。", Price: 24, ThumbnailUrl: "https://picsum.photos/400/220?2", IsPublished: true, avgRating: 4.8, reviewCount: 87 },
  { ProductId: 3, CategoryId: 4, Name: "Notion 工作系統", Description: "完整個人生產力模板，包含週計劃、專案追蹤、讀書清單一體化設計。", Price: 19, ThumbnailUrl: "https://picsum.photos/400/220?3", IsPublished: true, avgRating: 4.7, reviewCount: 65 },
  { ProductId: 4, CategoryId: 2, Name: "Midjourney Style Pack", Description: "100 組風格化 Midjourney V6 提示詞，包含賽博龐克、極簡、油畫等風格。", Price: 15, ThumbnailUrl: "https://picsum.photos/400/220?4", IsPublished: true, avgRating: 5.0, reviewCount: 43 },
  { ProductId: 5, CategoryId: 6, Name: "React Component Lib", Description: ".NET + React 前後端範本，含 JWT Auth、API 結構與部署設定。", Price: 39, ThumbnailUrl: "https://picsum.photos/400/220?5", IsPublished: true, avgRating: 4.6, reviewCount: 32 },
  { ProductId: 6, CategoryId: 5, Name: "UI/UX 設計完整指南", Description: "200 頁電子書，從使用者研究到 Figma 原型，適合初學者到中階設計師。", Price: 18, ThumbnailUrl: "https://picsum.photos/400/220?6", IsPublished: true, avgRating: 4.8, reviewCount: 156 },
  { ProductId: 7, CategoryId: 4, Name: "Landing Page 模板包", Description: "8 套高轉換率登陸頁模板，HTML/CSS 單檔可用，含深色淺色主題。", Price: 22, ThumbnailUrl: "https://picsum.photos/400/220?7", IsPublished: true, avgRating: 4.7, reviewCount: 74 },
  { ProductId: 8, CategoryId: 6, Name: ".NET 8 API 啟動模板", Description: "包含 JWT、Repository Pattern、Swagger、Docker 的完整 WebAPI 專案範本。", Price: 29, ThumbnailUrl: "https://picsum.photos/400/220?8", IsPublished: true, avgRating: 5.0, reviewCount: 21 },
];

const reviews = [
  { ReviewId: 1, ProductId: 1, Rating: 5, Comment: "非常實用，提示詞品質超出預期，直接用到了工作中！", CreatedAt: "2026-03-10", DisplayName: "James L." },
  { ReviewId: 2, ProductId: 1, Rating: 5, Comment: "整理得很有系統，買到賺到。", CreatedAt: "2026-03-08", DisplayName: "Amy C." },
  { ReviewId: 3, ProductId: 1, Rating: 4, Comment: "內容很豐富，如果能再多一些中文範例就更好了。", CreatedAt: "2026-03-05", DisplayName: "Kevin W." },
];

// ─── CONTEXT ─────────────────────────────────────────────────────────────────
const AppContext = createContext(null);

function AppProvider({ children }) {
  const [page, setPage] = useState("home");
  const [detailId, setDetailId] = useState(null);
  const [user, setUser] = useState(null);
  const [authProvider, setAuthProvider] = useState(null);
  const [sessionCart, setSessionCart] = useState([]);
  const [orders, setOrders] = useState([]);
  const [activeCat, setActiveCat] = useState(1);
  const [loginForCheckout, setLoginForCheckout] = useState(false);
  const [toasts, setToasts] = useState([]);
  const [loginOpen, setLoginOpen] = useState(false);
  const [checkoutOpen, setCheckoutOpen] = useState(false);

  const showPage = useCallback((name, id) => {
    setPage(name);
    if (id !== undefined) setDetailId(id);
    window.scrollTo({ top: 0, behavior: "smooth" });
  }, []);

  const showToast = useCallback((icon, msg) => {
    const id = Date.now() + Math.random();
    setToasts(p => [...p, { id, icon, msg }]);
    setTimeout(() => setToasts(p => p.filter(t => t.id !== id)), 3000);
  }, []);

  const isGuest = () => !user;

  const addToCart = useCallback((productId) => {
    setSessionCart(prev => {
      if (prev.includes(productId)) { showToast("ℹ️", "已在購物車中"); return prev; }
      const p = products.find(x => x.ProductId === productId);
      const hint = !user ? "（結帳時需要登入）" : "";
      showToast("🛒", `${p.Name} 已加入購物車 ${hint}`);
      return [...prev, productId];
    });
  }, [user, showToast]);

  const removeFromCart = useCallback((productId) => {
    setSessionCart(p => p.filter(id => id !== productId));
    showToast("🗑️", "已從購物車移除");
  }, [showToast]);

  const openLogin = useCallback(() => setLoginOpen(true), []);
  const closeLogin = useCallback(() => { setLoginOpen(false); setLoginForCheckout(false); }, []);
  const openLoginForCheckout = useCallback(() => { setLoginForCheckout(true); setLoginOpen(true); }, []);

  const loginAs = useCallback((userData, provider) => {
    const wasCheckout = loginForCheckout;
    setUser(userData); setAuthProvider(provider);
    setLoginOpen(false); setLoginForCheckout(false);
    showToast("👋", `歡迎，${userData.DisplayName}！`);
    if (wasCheckout) {
      setTimeout(() => {
        showToast("🛒", "購物車已保留，繼續結帳...");
        setTimeout(() => setCheckoutOpen(true), 800);
      }, 400);
    }
  }, [loginForCheckout, showToast]);

  const logout = useCallback(() => {
    setUser(null); setAuthProvider(null); setSessionCart([]);
    showPage("home"); showToast("👋", "已登出");
  }, [showPage, showToast]);

  const checkout = useCallback(() => {
    if (!sessionCart.length) { showToast("⚠️", "購物車是空的"); return; }
    if (!user) { openLoginForCheckout(); return; }
    setCheckoutOpen(true);
  }, [sessionCart.length, user, showToast, openLoginForCheckout]);

  const pay = useCallback((provider) => {
    setCheckoutOpen(false);
    const validItems = sessionCart
      .map(pid => products.find(x => x.ProductId === pid && x.IsPublished))
      .filter(Boolean);
    const orderItems = validItems.map((p, i) => ({
      OrderItemId: Date.now() + i, ProductId: p.ProductId,
      ProductName: p.Name, UnitPrice: p.Price, Quantity: 1, SubTotal: p.Price,
    }));
    const total = orderItems.reduce((s, i) => s + i.SubTotal, 0);
    const order = {
      OrderId: Date.now(),
      UserId: user.UserId,
      OrderNo: "DV-" + Math.random().toString(36).slice(2, 8).toUpperCase(),
      TotalAmount: total, Status: "Paid",
      CreatedAt: new Date().toLocaleDateString("zh-TW"),
      items: orderItems,
      Payment: { Provider: provider, TransactionId: "TXN-" + Date.now(), Amount: total, Status: "Paid" },
    };
    setOrders(p => [order, ...p]);
    setSessionCart([]);
    showPage("orders");
    showToast("✅", `付款成功！訂單 ${order.OrderNo} 已建立`);
  }, [sessionCart, user, showPage, showToast]);

  return (
    <AppContext.Provider value={{
      page, showPage, detailId,
      user, authProvider, isGuest,
      sessionCart, addToCart, removeFromCart,
      orders, activeCat, setActiveCat,
      loginForCheckout, toasts,
      loginOpen, openLogin, closeLogin, openLoginForCheckout,
      checkoutOpen, setCheckoutOpen,
      loginAs, logout, checkout, pay, showToast,
    }}>
      {children}
    </AppContext.Provider>
  );
}

const useApp = () => useContext(AppContext);

// ─── HELPERS ──────────────────────────────────────────────────────────────────
const getCatName = id => (categories.find(c => c.CategoryId === id) || {}).Name || "";

function Stars({ rating, size = "0.75rem" }) {
  const full = Math.round(rating);
  return <span className="stars" style={{ fontSize: size }}>{"★".repeat(full)}{"☆".repeat(5 - full)}</span>;
}

// ─── HEADER ───────────────────────────────────────────────────────────────────
function Header() {
  const { showPage, sessionCart, user, openLogin, logout } = useApp();
  return (
    <header className="dv-header">
      <div className="dv-logo" onClick={() => showPage("home")}>Digital<span>Vault</span></div>
      <nav className="dv-nav">
        <a onClick={() => showPage("home")}>首頁</a>
        <a onClick={() => showPage("store")}>商店</a>
        <button className="nav-cart" onClick={() => showPage("cart")}>
          🛒 購物車 <span className="cart-badge">{sessionCart.length}</span>
        </button>
        {!user ? (
          <>
            <div className="guest-badge"><div className="guest-dot" />訪客</div>
            <button className="btn-login" onClick={openLogin}>登入 / 註冊</button>
          </>
        ) : (
          <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
            <div className="nav-avatar" onClick={() => showPage("profile")} title={user.DisplayName}>
              {user.DisplayName[0].toUpperCase()}
            </div>
            <button className="nav-cart" style={{ fontSize: "0.78rem" }} onClick={() => showPage("orders")}>我的訂單</button>
            <button className="btn-login" style={{ background: "var(--surface)", color: "var(--muted)", border: "1px solid var(--border)" }} onClick={logout}>登出</button>
          </div>
        )}
      </nav>
    </header>
  );
}

// ─── PRODUCT CARD ─────────────────────────────────────────────────────────────
function ProductCard({ product, onDetail }) {
  const { sessionCart, addToCart } = useApp();
  const inCart = sessionCart.includes(product.ProductId);
  return (
    <div className="card">
      <div className="card-img" onClick={() => onDetail(product.ProductId)}>
        <img src={product.ThumbnailUrl} alt={product.Name} loading="lazy" />
        <span className="card-cat">{getCatName(product.CategoryId)}</span>
      </div>
      <div className="card-body">
        <div className="card-title" onClick={() => onDetail(product.ProductId)}>{product.Name}</div>
        <div className="card-desc">{product.Description}</div>
        <div className="card-rating">
          <Stars rating={product.avgRating} />
          {product.avgRating} ({product.reviewCount} 則評論)
        </div>
        <div className="card-footer">
          <span className="card-price">${product.Price}</span>
          <button className={`btn-cart ${inCart ? "added" : ""}`} onClick={e => { e.stopPropagation(); addToCart(product.ProductId); }}>
            {inCart ? "✓ 已加入" : "加入購物車"}
          </button>
        </div>
      </div>
    </div>
  );
}

// ─── HOME PAGE ────────────────────────────────────────────────────────────────
const STATS = [
  { num: "200+", label: "數位商品" },
  { num: "4.9★", label: "平均評分" },
  { num: "5K+", label: "滿意客戶" },
  { num: "即時", label: "下載交付" },
];

function HomePage() {
  const { showPage, openLogin } = useApp();
  return (
    <>
      <section className="hero">
        <div className="hero-bg" /><div className="hero-grid" />
        <div className="hero-content">
          <div className="hero-tag">✦ 數位資產商店</div>
          <h2>探索頂級<br /><em>數位資源</em></h2>
          <p>AI 提示詞 · UI 套件 · 模板 · 程式資源<br />即購即用，無需等待配送</p>
          <div className="hero-btns">
            <button className="btn-primary" onClick={() => showPage("store")}>瀏覽商店</button>
            <button className="btn-outline" onClick={openLogin}>登入帳號</button>
          </div>
        </div>
      </section>
      <div className="stats">
        {STATS.map(s => (
          <div key={s.label} className="stat">
            <div className="stat-num">{s.num}</div>
            <div className="stat-label">{s.label}</div>
          </div>
        ))}
      </div>
      <div className="section">
        <div className="section-header">
          <div className="section-title">熱門商品 <span>精選</span></div>
          <button className="btn-outline" style={{ padding: "8px 18px", fontSize: "0.82rem" }} onClick={() => showPage("store")}>查看全部 →</button>
        </div>
        <div className="products" style={{ padding: 0 }}>
          {products.slice(0, 4).map(p => <ProductCard key={p.ProductId} product={p} onDetail={id => showPage("detail", id)} />)}
        </div>
      </div>
    </>
  );
}

// ─── STORE PAGE ───────────────────────────────────────────────────────────────
function StorePage() {
  const { activeCat, setActiveCat, showPage } = useApp();
  const list = activeCat === 1 ? products.filter(p => p.IsPublished) : products.filter(p => p.IsPublished && p.CategoryId === activeCat);
  return (
    <>
      <div className="page-title">商店 <span>全部商品</span></div>
      <div className="cat-nav">
        {categories.map(c => (
          <button key={c.CategoryId} className={`cat-btn ${c.CategoryId === activeCat ? "active" : ""}`} onClick={() => setActiveCat(c.CategoryId)}>{c.Name}</button>
        ))}
      </div>
      <div className="products">
        {list.length === 0
          ? <div className="empty-state"><div className="empty-icon">📦</div><h3>此分類暫無商品</h3></div>
          : list.map(p => <ProductCard key={p.ProductId} product={p} onDetail={id => showPage("detail", id)} />)
        }
      </div>
    </>
  );
}

// ─── DETAIL PAGE ──────────────────────────────────────────────────────────────
const INCLUDES = ["即時數位下載", "永久存取權限", "購買憑證（OrderItems 記錄）", "30 天退款保障"];

function DetailPage({ productId }) {
  const { sessionCart, addToCart, isGuest } = useApp();
  const p = products.find(x => x.ProductId === productId);
  if (!p) return null;
  const inCart = sessionCart.includes(p.ProductId);
  const pReviews = reviews.filter(r => r.ProductId === productId);
  return (
    <>
      <div className="detail-layout">
        <div className="detail-img"><img src={p.ThumbnailUrl} alt={p.Name} /></div>
        <div className="detail-info">
          <div className="detail-cat">{getCatName(p.CategoryId)}</div>
          <div className="detail-title">{p.Name}</div>
          <div className="detail-rating">
            <Stars rating={p.avgRating} size="1rem" />
            <strong>{p.avgRating}</strong><span>({p.reviewCount} 則評論)</span>
          </div>
          <p className="detail-desc">{p.Description}</p>
          <div className="detail-includes">
            <h4>包含內容</h4>
            {INCLUDES.map(item => <div key={item} className="include-item">{item}</div>)}
          </div>
          <div className="detail-price-row">
            <span className="detail-price">${p.Price}</span>
            <button className={`btn-add-cart ${inCart ? "added" : ""}`} onClick={() => addToCart(p.ProductId)}>
              {inCart ? "✓ 已加入購物車" : "加入購物車"}
            </button>
          </div>
          <div style={{ fontSize: "0.78rem", color: "var(--muted)" }}>
            {isGuest() ? "🔒 結帳時需要登入，加入購物車無需帳號" : "付款後狀態：Orders.Status = Paid → Completed"}
          </div>
        </div>
      </div>
      <div className="reviews-section">
        <div className="section-title" style={{ marginBottom: "20px" }}>用戶評論 <span style={{ color: "var(--cyan)" }}>(Reviews)</span></div>
        {pReviews.length === 0
          ? <div className="empty-state" style={{ padding: "40px" }}><div className="empty-icon">💬</div><h3>尚無評論</h3><p>購買後即可留下評論</p></div>
          : pReviews.map(r => (
            <div key={r.ReviewId} className="review-card">
              <div className="review-header">
                <div className="reviewer-avatar">{r.DisplayName[0]}</div>
                <div><div className="reviewer-name">{r.DisplayName}</div><Stars rating={r.Rating} size="0.7rem" /></div>
                <span className="review-date">{r.CreatedAt}</span>
              </div>
              <div className="review-text">{r.Comment}</div>
            </div>
          ))
        }
      </div>
    </>
  );
}

// ─── CART PAGE ────────────────────────────────────────────────────────────────
function CartPage() {
  const { sessionCart, removeFromCart, isGuest, openLoginForCheckout, checkout, showPage } = useApp();
  const cartProds = sessionCart.map(pid => products.find(x => x.ProductId === pid)).filter(Boolean);
  const validProds = cartProds.filter(p => p.IsPublished);
  const total = validProds.reduce((s, p) => s + p.Price, 0);
  return (
    <>
      <div className="page-title">購物車 <span>結帳</span></div>
      <div className="cart-layout">
        <div>
          {isGuest() && sessionCart.length > 0 && (
            <div className="guest-checkout-bar">
              <div><p>🔒 結帳前需要登入帳號</p><span>商品已暫存在 Session，登入後不會消失</span></div>
              <button className="btn-primary" style={{ padding: "8px 20px", fontSize: "0.85rem", whiteSpace: "nowrap" }} onClick={openLoginForCheckout}>登入 / 註冊</button>
            </div>
          )}
          {sessionCart.length === 0
            ? <div className="empty-state"><div className="empty-icon">🛒</div><h3>購物車是空的</h3><p style={{ marginTop: "8px" }}><button className="btn-primary" onClick={() => showPage("store")}>去逛逛</button></p></div>
            : cartProds.map(p => (
              <div key={p.ProductId} className="cart-item" style={!p.IsPublished ? { opacity: 0.5 } : {}}>
                <div className="cart-item-img"><img src={p.ThumbnailUrl} alt={p.Name} /></div>
                <div className="cart-item-info">
                  <div className="cart-item-name" style={!p.IsPublished ? { textDecoration: "line-through" } : {}}>{p.Name}</div>
                  {p.IsPublished ? <div className="cart-item-cat">{getCatName(p.CategoryId)}</div> : <div className="cart-item-cat" style={{ color: "var(--danger)" }}>此商品已下架</div>}
                </div>
                {p.IsPublished && <div className="cart-item-price">${p.Price}</div>}
                <button className="btn-remove" onClick={() => removeFromCart(p.ProductId)}>✕</button>
              </div>
            ))
          }
        </div>
        <div className="cart-summary">
          <div className="summary-title">訂單摘要</div>
          {validProds.map(p => <div key={p.ProductId} className="summary-row"><span>{p.Name}</span><span>${p.Price}</span></div>)}
          <div className="summary-row total"><span>總計</span><span>${total}</span></div>
          <button className="btn-checkout" onClick={checkout}>前往付款</button>
          <div style={{ textAlign: "center", fontSize: "0.75rem", color: "var(--muted)", marginTop: "12px" }}>🔒 付款後即時取得下載連結</div>
        </div>
      </div>
    </>
  );
}

// ─── ORDERS PAGE ──────────────────────────────────────────────────────────────
function OrdersPage() {
  const { orders, isGuest, openLogin, showPage } = useApp();
  const statusClass = s => s === "Paid" ? "status-paid" : s === "Completed" ? "status-completed" : "status-unpaid";
  if (isGuest()) return <div className="orders-page"><div className="empty-state"><div className="empty-icon">🔐</div><h3>請先登入查看訂單</h3><p style={{ marginTop: "12px" }}><button className="btn-primary" onClick={openLogin}>登入</button></p></div></div>;
  if (!orders.length) return <div className="orders-page"><div className="empty-state"><div className="empty-icon">📋</div><h3>尚無訂單記錄</h3><p style={{ marginTop: "8px" }}><button className="btn-primary" onClick={() => showPage("store")}>開始購物</button></p></div></div>;
  return (
    <div className="orders-page">
      {orders.map(order => (
        <div key={order.OrderId} className="order-card">
          <div className="order-header">
            <div className="order-no">訂單 {order.OrderNo}</div>
            <div className={`order-status ${statusClass(order.Status)}`}>{order.Status}</div>
          </div>
          <div className="order-items-list">
            {order.items.map(item => (
              <div key={item.OrderItemId} className="order-item-chip">{item.ProductName}<span>×{item.Quantity}</span><strong>${item.SubTotal}</strong></div>
            ))}
          </div>
          <div className="order-footer">
            <div className="order-date">📅 {order.CreatedAt} · {order.Payment.Provider}</div>
            <div className="order-total">${order.TotalAmount}</div>
          </div>
        </div>
      ))}
    </div>
  );
}

// ─── PROFILE PAGE ─────────────────────────────────────────────────────────────
function ProfilePage() {
  const { user, authProvider, orders, sessionCart, isGuest, showPage } = useApp();
  if (isGuest()) return <div className="profile-page"><div className="empty-state"><div className="empty-icon">🔐</div><h3>請先登入</h3></div></div>;
  return (
    <div className="profile-page">
      <div className="profile-card">
        <div className="profile-avatar">{user.DisplayName[0].toUpperCase()}</div>
        <div>
          <div className="profile-name">{user.DisplayName}</div>
          <div className="profile-email">{user.Email}</div>
          <div className="provider-badge">{authProvider === "google" ? "🔵 Google 登入" : "🔑 Email 登入"} · Provider = "{authProvider}"</div>
        </div>
      </div>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "14px" }}>
        {[{ value: orders.length, label: "訂單數" }, { value: sessionCart.length, label: "購物車" }].map(({ value, label }) => (
          <div key={label} style={{ background: "var(--card)", border: "1px solid var(--border)", borderRadius: "12px", padding: "20px", textAlign: "center" }}>
            <div style={{ fontFamily: "'Syne',sans-serif", fontSize: "1.8rem", fontWeight: 800, color: "var(--cyan)" }}>{value}</div>
            <div style={{ fontSize: "0.8rem", color: "var(--muted)", marginTop: "4px" }}>{label}</div>
          </div>
        ))}
      </div>
      <div style={{ marginTop: "20px" }}>
        <button className="btn-outline" style={{ width: "100%", padding: "12px" }} onClick={() => showPage("orders")}>查看我的訂單</button>
      </div>
    </div>
  );
}

// ─── LOGIN MODAL ──────────────────────────────────────────────────────────────
function LoginModal() {
  const { loginOpen, closeLogin, loginAs, loginForCheckout } = useApp();
  const [mode, setMode] = useState("login");
  const [email, setEmail] = useState("");
  const [name, setName] = useState("");
  const [regEmail, setRegEmail] = useState("");
  if (!loginOpen) return null;
  const title = loginForCheckout ? "結帳前請先登入" : mode === "login" ? "歡迎回來" : "建立帳號";
  const sub = loginForCheckout ? "登入後購物車內容不會消失" : mode === "login" ? "登入你的帳號繼續購物" : "開始你的數位資產之旅";
  return (
    <div className="overlay" onClick={e => e.target === e.currentTarget && closeLogin()}>
      <div className="modal">
        <button className="modal-close" onClick={closeLogin}>✕</button>
        <div className="modal-logo">DIGITAL VAULT</div>
        <h3>{title}</h3>
        <p className="modal-sub">{sub}</p>
        {loginForCheckout && <div className="checkout-login-hint">🛒 你的購物車商品已暫存，登入後即可繼續結帳</div>}
        <button className="btn-google" onClick={() => loginAs({ UserId: 1, Email: "user@gmail.com", DisplayName: "Google 使用者", Role: "user" }, "google")}>
          <div className="google-icon" />使用 Google 帳號登入
        </button>
        <div className="divider">或使用 Email</div>
        {mode === "login" ? (
          <>
            <div className="form-group"><label>電子郵件</label><input type="email" placeholder="you@example.com" value={email} onChange={e => setEmail(e.target.value)} /></div>
            <div className="form-group"><label>密碼</label><input type="password" placeholder="••••••••" /></div>
            <button className="btn-submit" onClick={() => loginAs({ UserId: 2, Email: email || "user@example.com", DisplayName: (email || "user@example.com").split("@")[0], Role: "user" }, "local")}>登入</button>
            <div className="modal-switch">還沒有帳號？<a onClick={() => setMode("register")}>立即註冊</a></div>
          </>
        ) : (
          <>
            <div className="form-group"><label>顯示名稱</label><input type="text" placeholder="你的名字" value={name} onChange={e => setName(e.target.value)} /></div>
            <div className="form-group"><label>電子郵件</label><input type="email" placeholder="you@example.com" value={regEmail} onChange={e => setRegEmail(e.target.value)} /></div>
            <div className="form-group"><label>密碼</label><input type="password" placeholder="至少 8 字元" /></div>
            <button className="btn-submit" onClick={() => loginAs({ UserId: 3, Email: regEmail || "new@example.com", DisplayName: name || "新用戶", Role: "user" }, "local")}>建立帳號</button>
            <div className="modal-switch">已有帳號？<a onClick={() => setMode("login")}>登入</a></div>
          </>
        )}
      </div>
    </div>
  );
}

// ─── CHECKOUT MODAL ───────────────────────────────────────────────────────────
function CheckoutModal() {
  const { checkoutOpen, setCheckoutOpen, pay } = useApp();
  if (!checkoutOpen) return null;
  return (
    <div className="overlay" onClick={e => e.target === e.currentTarget && setCheckoutOpen(false)}>
      <div className="modal">
        <button className="modal-close" onClick={() => setCheckoutOpen(false)}>✕</button>
        <div className="modal-logo">DIGITAL VAULT</div>
        <h3>選擇付款方式</h3>
        <p className="modal-sub">台灣金流 · 安全加密交易</p>
        <div style={{ display: "flex", flexDirection: "column", gap: "12px", marginTop: "20px" }}>
          <button className="btn-google" style={{ background: "#1c2340", color: "var(--text)", border: "1px solid var(--border)" }} onClick={() => pay("ECPay")}>💳 綠界 ECPay 信用卡</button>
          <button className="btn-google" style={{ background: "#00C300", color: "white" }} onClick={() => pay("LinePay")}>💚 LINE Pay</button>
        </div>
        <div style={{ textAlign: "center", fontSize: "0.75rem", color: "var(--muted)", marginTop: "16px" }}>付款完成後訂單狀態自動更新為 Paid</div>
      </div>
    </div>
  );
}

// ─── TOAST ────────────────────────────────────────────────────────────────────
function ToastContainer() {
  const { toasts } = useApp();
  return (
    <div className="toast-wrap">
      {toasts.map(t => (
        <div key={t.id} className="toast">
          <span className="toast-icon">{t.icon}</span>
          <span>{t.msg}</span>
        </div>
      ))}
    </div>
  );
}

// ─── APP ROOT ─────────────────────────────────────────────────────────────────
function AppInner() {
  const { page, detailId, showPage } = useApp();
  return (
    <div style={{ background: "var(--bg)", minHeight: "100vh", color: "var(--text)", fontFamily: "'DM Sans', sans-serif" }}>
      <Header />
      <main>
        {page === "home"    && <HomePage />}
        {page === "store"   && <StorePage />}
        {page === "detail"  && <DetailPage productId={detailId} />}
        {page === "cart"    && <CartPage />}
        {page === "orders"  && <><div className="page-title">我的 <span>訂單</span></div><OrdersPage /></>}
        {page === "profile" && <><div className="page-title">會員 <span>中心</span></div><ProfilePage /></>}
      </main>
      <footer className="dv-footer">
        <div className="footer-logo">DIGITAL VAULT</div>
        <div className="footer-copy">© 2026 Digital Vault · 數位商品即購即用</div>
      </footer>
      <LoginModal />
      <CheckoutModal />
      <ToastContainer />
    </div>
  );
}

export default function App() {
  return (
    <>
      <style>{CSS}</style>
      <AppProvider>
        <AppInner />
      </AppProvider>
    </>
  );
}

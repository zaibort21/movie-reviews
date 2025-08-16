// Datos de productos (5 modelos, stock 12 c/u)
const PRODUCTS = [
  {
    id: 'beisbolera',
    title: 'Classic',
    price: 55000,
    description: 'Gorra clásica, versátil y elegante.',
    images: ['img/modelo1.jpg'],
    stock: 12,
    colors: ['Negro','Blanco','Beige'],
    sizes: ['S','M','L']
  },
  {
    id: 'Gorra plana',
    title: 'clasica',
    price: 65000,
    description: 'Gorra clásica, versátil y elegante.',
    images: ['img/modelo2.jpg'],
    stock: 12,
    colors: ['Azul Marino','Gris','Negro'],
    sizes: ['S','M','L']
  },
  {
    id: 'plana',
    title: 'clasica',
    price: 65000,
    description: 'Acabados premium, hilo dorado disponible.',
    images: ['img/modelo3.jpg'],
    stock: 12,
    colors: ['Negro','Dorado','Blanco'],
    sizes: ['S','M','L']
  },
  {
    id: 'Plana',
    title: 'Clasica',
    price: 65000,
    description: 'Estilo retro con paleta de temporada.',
    images: ['img/modelo4.jpg'],
    stock: 12,
    colors: ['Azul Claro','Bordeaux','Blanco'],
    sizes: ['S','M','L']
  },
  {
    id: 'plana',
    title: 'Clasica',
    price: 65000,
    description: 'Perfectas para equipos y merchandising.',
    images: ['img/modelo5.jpg'],
    stock: 12,
    colors: ['Marino','Negro','Gris'],
    sizes: ['S','M','L']
  },
  {
    id: 'reloj',
    title: 'Clasica',
    price: 70000,
    description: 'Accesorios para tu estilo',
    images: ['img/modelo5.jpg'],
    stock: 12,
    colors: ['Marino','Negro','Gris'],
    sizes: ['S','M','L']
  },
  {
    id: 'personalizada',
    title: 'Clasica',
    price: 60000,
    description: 'modifica tu gorra a tu estilo',
    images: ['img/modelo5.jpg'],
    stock: 12,
    colors: ['Marino','Negro','Gris'],
    sizes: ['S','M','L']
  },
  {
    id: 'plana niños',
    title: 'Clasica',
    price: 65000,
    description: 'Perfectas para equipos y merchandising.',
    images: ['img/modelo5.jpg'],
    stock: 10,
    colors: ['Marino','Negro','Gris'],
    sizes: ['S','M','L']
  },
  {
    id: 'beisbolera',
    title: 'Clasica',
    price: 55000,
    description: 'Perfectas para equipos y merchandising.',
    images: ['img/modelo5.jpg'],
    stock: 10,
    colors: ['Marino','Negro','Gris'],
    sizes: ['S','M','L']
  },
  {
    id: 'beisbolera',
    title: 'Clasica',
    price: 55000,
    description: 'Perfectas para equipos y merchandising.',
    images: ['img/modelo5.jpg'],
    stock: 10,
    colors: ['Marino','Negro','Gris'],
    sizes: ['S','M','L']
  },
  {
    id: 'beisbolera',
    title: 'Clasica',
    price: 55000,
    description: 'Perfectas para equipos y merchandising.',
    images: ['img/modelo5.jpg'],
    stock: 10,
    colors: ['Marino','Negro','Gris'],
    sizes: ['S','M','L']
  }
];

// Estado del carrito
let cart = [];

// utils
const formatCOP = (n) => new Intl.NumberFormat('es-CO',{style:'currency',currency:'COP',maximumFractionDigits:0}).format(n);

// Render productos
const grid = document.getElementById('products-grid');
function renderProducts(){
  grid.innerHTML = '';
  PRODUCTS.forEach(p => {
    const card = document.createElement('article');
    card.className = 'card';
    card.innerHTML = `
      <div class="product-media"><img src="${p.images[0]}" alt="${p.title}"></div>
      <h4 class="product-title">${p.title}</h4>
      <p class="product-price">${formatCOP(p.price)}</p>
      <p class="muted">${p.description}</p>
      <div class="variants">
        <select class="color-select" data-id="${p.id}">
          ${p.colors.map(c=>`<option value="${c}">${c}</option>`).join('')}
        </select>
        <select class="size-select" data-id="${p.id}">
          ${p.sizes.map(s=>`<option value="${s}">${s}</option>`).join('')}
        </select>
        <input class="qty-input" data-id="${p.id}" type="number" min="1" max="${p.stock}" value="1" style="width:64px;padding:.4rem;border-radius:6px;border:1px solid #e6e6e6"/>
      </div>
      <div class="add-cart">
        <button class="btn primary add-to-cart" data-id="${p.id}">Agregar</button>
        <small style="color:var(--muted);margin-left:.5rem">Stock: ${p.stock}</small>
      </div>
    `;
    grid.appendChild(card);
  });

  document.querySelectorAll('.add-to-cart').forEach(btn=>{
    btn.addEventListener('click', (e)=>{
      const id = e.currentTarget.dataset.id;
      const prod = PRODUCTS.find(x=>x.id===id);
      const color = document.querySelector(`.color-select[data-id="${id}"]`).value;
      const size = document.querySelector(`.size-select[data-id="${id}"]`).value;
      const qty = parseInt(document.querySelector(`.qty-input[data-id="${id}"]`).value,10);
      addToCart({id:prod.id,title:prod.title,price:prod.price,color,size,qty,stock:prod.stock});
    });
  });
}

function addToCart(item){
  // valid stock
  const prod = PRODUCTS.find(p=>p.id===item.id);
  // check existing qty in cart
  const existingQty = cart.filter(c=>c.id===item.id && c.color===item.color && c.size===item.size).reduce((s,i)=>s+i.qty,0);
  if(existingQty + item.qty > prod.stock){
    alert('No hay suficiente stock para ese producto/variación.');
    return;
  }
  // merge same variant
  const existing = cart.find(c=>c.id===item.id && c.color===item.color && c.size===item.size);
  if(existing){ existing.qty += item.qty; } else { cart.push(item); }
  updateCartUI();
}

function updateCartUI(){
  document.getElementById('cart-count').innerText = cart.reduce((s,i)=>s+i.qty,0);
  // render items
  const itemsWrap = document.getElementById('cart-items');
  if(!itemsWrap) return;
  itemsWrap.innerHTML = '';
  cart.forEach((it,idx)=>{
    const div = document.createElement('div');
    div.className='cart-item';
    div.style.padding='8px 0';
    div.innerHTML = `
      <div style="display:flex;justify-content:space-between;align-items:center">
        <div><strong>${it.title}</strong><div style="font-size:.9rem;color:var(--muted)">${it.color} • ${it.size}</div></div>
        <div>${formatCOP(it.price*it.qty)}</div>
      </div>
      <div style="margin-top:.4rem;display:flex;gap:.5rem;align-items:center">
        <button data-idx="${idx}" class="btn outline qty-decrease">-</button>
        <div>${it.qty}</div>
        <button data-idx="${idx}" class="btn outline qty-increase">+</button>
        <button data-idx="${idx}" class="btn" style="margin-left:auto;background:transparent;border:0;color:red" id="remove-${idx}">Eliminar</button>
      </div>
    `;
    itemsWrap.appendChild(div);
  });

  // attach handlers
  document.querySelectorAll('.qty-increase').forEach(b=>{
    b.addEventListener('click', (e)=>{
      const i = parseInt(e.currentTarget.dataset.idx,10);
      const prod = PRODUCTS.find(p=>p.id===cart[i].id);
      if(cart[i].qty + 1 > prod.stock){ alert('Stock insuficiente'); return; }
      cart[i].qty++; updateCartUI();
    });
  });
  document.querySelectorAll('.qty-decrease').forEach(b=>{
    b.addEventListener('click', (e)=>{
      const i = parseInt(e.currentTarget.dataset.idx,10);
      if(cart[i].qty > 1){ cart[i].qty--; } else { cart.splice(i,1); }
      updateCartUI();
    });
  });
  cart.forEach((c, idx)=> {
    const btn = document.getElementById(`remove-${idx}`);
    if(btn) btn.addEventListener('click', ()=>{ cart.splice(idx,1); updateCartUI(); });
  });

  // subtotal
  const subtotal = cart.reduce((s,i)=>s+i.price*i.qty,0);
  document.getElementById('cart-subtotal').innerText = formatCOP(subtotal);
  // update cart items small
  if(cart.length === 0){
    document.getElementById('cart-items').innerHTML = '<p style="color:var(--muted)">Tu carrito está vacío.</p>';
  }
}

// Cart open/close
const cartPanel = document.getElementById('carrito');
document.getElementById('cart-btn').addEventListener('click', (e)=>{
  e.preventDefault();
  cartPanel.setAttribute('aria-hidden', 'false');
});
document.getElementById('close-cart').addEventListener('click', ()=>{
  cartPanel.setAttribute('aria-hidden','true');
});

// WhatsApp checkout
document.getElementById('checkout-whatsapp').addEventListener('click', ()=>{
  if(cart.length === 0) { alert('Tu carrito está vacío'); return; }
  const phone = '573115477984'; // REEMPLAZA: tu numero en formato internacional sin +
  let text = `Hola! quiero hacer un pedido:%0A`;
  cart.forEach(item => {
    text += `- ${item.title} | ${item.color} | ${item.size} x${item.qty} => ${formatCOP(item.price*item.qty)}%0A`;
  });
  const subtotal = cart.reduce((s,i)=>s+i.price*i.qty,0);
  text += `%0ASubtotal: ${formatCOP(subtotal)}%0AEnvío: (indicar)%0ATotal: ${formatCOP(subtotal)}%0A%0ANombre:%0ADirección:%0ANota:`; 
  const url = `https://wa.me/${phone}?text=${text}`;
  window.open(url, '_blank');
});

// Stripe checkout (cliente -> requiere endpoint en servidor)
document.getElementById('checkout-stripe').addEventListener('click', async ()=>{
  if(cart.length === 0) { alert('Tu carrito está vacío'); return; }
  // Prepara items para enviar al servidor
  const line_items = cart.map(it => ({
    price_data: {
      currency: 'cop',
      product_data: { name: `${it.title} - ${it.color} - ${it.size}` },
      unit_amount: it.price // NOTE: Stripe expects amount in cents for many currencies; check docs for COP.
    },
    quantity: it.qty
  }));
  // Mandar al servidor para crear session (endpoint ejemplo: /create-checkout-session)
  try {
    const res = await fetch('/create-checkout-session', {
      method:'POST',
      headers:{'Content-Type':'application/json'},
      body: JSON.stringify({ line_items })
    });
    const data = await res.json();
    if(data.sessionId){
      const stripe = Stripe('pk_test_REPLACE'); // REEMPLAZA con tu public key
      stripe.redirectToCheckout({ sessionId: data.sessionId });
    } else {
      alert('Error al crear sesión de pago');
    }
  } catch(err){
    console.error(err);
    alert('Error al contactar el servidor de pagos');
  }
});

// CTA WhatsApp header/footer
document.getElementById('cta-whatsapp').addEventListener('click', (e)=>{
  e.preventDefault();
  window.location.href = '#carrito';
  cartPanel.setAttribute('aria-hidden','false');
});
document.getElementById('whatsapp-footer').addEventListener('click', (e)=>{
  e.preventDefault();
  // abrir chat vacío
  window.open('https://wa.me/573115477984', '_blank'); // REEMPLAZA
});

// Carrusel de imágenes del catálogo
const carrusel = document.getElementById('carrusel');
if (carrusel) {
  // Toma la primera imagen de cada producto
  let images = PRODUCTS.map(p => p.images[0]);
  // Duplica las imágenes para efecto infinito
  images = images.concat(images);
  carrusel.innerHTML = images.map(src => `<img src="${src}" alt="Gorra catálogo">`).join('');
}

// init
renderProducts();
updateCartUI();
document.getElementById('year').innerText = new Date().getFullYear();


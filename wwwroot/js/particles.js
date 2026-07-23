// Dark Fantasy Canvas Particle & Fog Engine for The Prince Rebellion

class ParticleEngine {
    constructor() {
        this.canvas = null;
        this.ctx = null;
        this.particles = [];
        this.animId = null;
        this.active = false;
    }

    init(canvasId) {
        this.canvas = document.getElementById(canvasId);
        if (!this.canvas) return;
        this.ctx = this.canvas.getContext('2d');
        this.resize();

        window.removeEventListener('resize', this.onResize);
        this.onResize = () => this.resize();
        window.addEventListener('resize', this.onResize);

        this.particles = [];
        const count = Math.min(Math.floor((window.innerWidth * window.innerHeight) / 12000), 80);
        for (let i = 0; i < count; i++) {
            this.particles.push(this.createParticle());
        }

        this.active = true;
        this.animate();
    }

    resize() {
        if (!this.canvas) return;
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
    }

    createParticle() {
        const isEmbers = Math.random() > 0.4;
        return {
            x: Math.random() * this.canvas.width,
            y: Math.random() * this.canvas.height,
            radius: isEmbers ? Math.random() * 2 + 1 : Math.random() * 45 + 15,
            color: isEmbers 
                ? `rgba(56, 189, 248, ${Math.random() * 0.6 + 0.2})`  // Sky Blue glowing ember
                : `rgba(30, 41, 59, ${Math.random() * 0.15 + 0.05})`, // Ambient fog cloud
            speedY: isEmbers ? -(Math.random() * 0.8 + 0.3) : -(Math.random() * 0.2 + 0.05),
            speedX: (Math.random() - 0.5) * 0.4,
            alpha: Math.random(),
            pulseSpeed: Math.random() * 0.02 + 0.005,
            isEmbers: isEmbers
        };
    }

    animate() {
        if (!this.active || !this.ctx || !this.canvas) return;
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

        for (let i = 0; i < this.particles.length; i++) {
            let p = this.particles[i];

            p.y += p.speedY;
            p.x += p.speedX;
            p.alpha += p.pulseSpeed;

            if (p.alpha > 1 || p.alpha < 0.1) p.pulseSpeed = -p.pulseSpeed;

            if (p.y < -50 || p.x < -50 || p.x > this.canvas.width + 50) {
                this.particles[i] = this.createParticle();
                this.particles[i].y = this.canvas.height + 20;
            }

            this.ctx.beginPath();
            this.ctx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
            this.ctx.fillStyle = p.color;
            this.ctx.shadowBlur = p.isEmbers ? 12 : 0;
            this.ctx.shadowColor = 'rgba(56, 189, 248, 0.8)';
            this.ctx.fill();
        }

        this.animId = requestAnimationFrame(() => this.animate());
    }

    stop() {
        this.active = false;
        if (this.animId) cancelAnimationFrame(this.animId);
    }
}

window.particleEngine = new ParticleEngine();

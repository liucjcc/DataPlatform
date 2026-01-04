export default class Sidebar {

    constructor(container) {
        this.container = container;
    }

    async mount() {
        await this.#loadHTML();
        this.#bind();
    }

    async #loadHTML() {
        const html = await fetch('/components/sidebar/sidebar.html')
            .then(r => r.text());

        this.container.innerHTML = html;

        this.sidebar = this.container.querySelector('#sidebar');
        this.toggleBtn = this.container.querySelector('.toggle-btn');
    }

    #bind() {
        this.toggleBtn.onclick = () => {
            this.sidebar.classList.toggle('collapsed');
        };
    }

    /** 可选：对外暴露状态 */
    isCollapsed() {
        return this.sidebar.classList.contains('collapsed');
    }

    collapse() {
        this.sidebar.classList.add('collapsed');
    }

    expand() {
        this.sidebar.classList.remove('collapsed');
    }
}

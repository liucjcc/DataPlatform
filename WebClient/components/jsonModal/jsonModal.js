
import JSONEditor from 'jsoneditor';
import 'jsoneditor/dist/jsoneditor.css';

export class JsonModal {
    constructor() {
        // 创建模态 DOM
        this.modal = document.createElement('div');
        this.modal.classList.add('json-modal');
        this.modal.innerHTML = `
            <div class="modal-content">
                <span class="close">&times;</span>
                <h3> 原始JSON数据查看器 </h3>
                <div class="jsoneditor-container" style="height:400px;"></div>
            </div>
        `;
        document.body.appendChild(this.modal);

        this.container = this.modal.querySelector('.jsoneditor-container');

        // 关闭事件
        this.modal.querySelector('.close').onclick = () => this.hide();
        this.modal.addEventListener('click', e => {
            if (e.target === this.modal) this.hide();
        });

        // 初始化 JSONEditor
        this.editor = new JSONEditor(this.container, {
            mode: 'view',
            mainMenuBar: false,
            navigationBar: true,
            statusBar: false
        });
    }

    show(jsonData) {
        this.editor.set(jsonData);
        this.modal.style.display = 'block';
    }

    hide() {
        this.modal.style.display = 'none';
    }
}

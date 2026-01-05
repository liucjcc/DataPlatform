export class ModbusConfigEditor {
    constructor(container, config) {
        this.container = container;
        this.config = config;
        this.editIndex = null;

        this.render();
        this.bindTableEvents();
        this.createModal();
    }

    /* ========== 渲染表格 ========== */
    render() {
        this.container.innerHTML = `
      <table class="modbus-config-table">
        <thead>
          <tr>
            <th>Address</th>
            <th>Name</th>
            <th>Type</th>
            <th>Length</th>
            <th>Scale</th>
            <th>Byte Order</th>
            <th>Description</th>
            <th>操作</th>
          </tr>
        </thead>
        <tbody>
          ${this.config.registers.map((r, i) => `
            <tr data-index="${i}">
              <td>${r.address}</td>
              <td>${r.name || ''}</td>
              <td>${r.type}</td>
              <td>${r.length || 1}</td>
              <td>${r.scale ?? ''}</td>
              <td>${r.byte_order || ''}</td>
              <td>${r.description || ''}</td>
              <td>
                <button class="btn-edit">编辑</button>
                <button class="btn-delete">删除</button>
              </td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `;
    }

    /* ========== 表格事件委托 ========== */
    bindTableEvents() {
        this.container.addEventListener('click', e => {
            const row = e.target.closest('tr[data-index]');
            if (!row) return;

            const index = Number(row.dataset.index);

            if (e.target.classList.contains('btn-edit')) {
                this.editRegister(index);
            }

            if (e.target.classList.contains('btn-delete')) {
                this.deleteRegister(index);
            }
        });
    }

    /* ========== 对外 API ========== */
    addRegister() {
        this.editIndex = null;
        this.openModal({
            address: 0,
            name: '',
            type: 'int16',
            length: 1,
            scale: '',
            byte_order: 'ABCD',
            description: ''
        });
    }

    /* ========== 内部操作 ========== */
    editRegister(index) {
        this.editIndex = index;
        this.openModal({ ...this.config.registers[index] });
    }

    deleteRegister(index) {
        if (!confirm('确定删除该寄存器定义？')) return;
        this.config.registers.splice(index, 1);
        this.render();
    }

    /* ========== 模态框 ========== */
    createModal() {
        this.modal = document.createElement('div');
        this.modal.className = 'modbus-modal hidden';
        this.modal.innerHTML = `
      <div class="modbus-mask"></div>
      <div class="modbus-dialog">
        <h3>寄存器编辑</h3>

        <label>Address <input id="m-address" type="number"></label>
        <label>Name <input id="m-name"></label>
        <label>Type <input id="m-type"></label>
        <label>Length <input id="m-length" type="number"></label>
        <label>Scale <input id="m-scale" type="number"></label>
        <label>Byte Order <input id="m-byte"></label>
        <label>Description <input id="m-desc"></label>

        <div class="actions">
          <button id="m-ok" type="button">确定</button>
          <button id="m-cancel" type="button">取消</button>
        </div>
      </div>
    `;
        document.body.appendChild(this.modal);

        const dialog = this.modal.querySelector('.modbus-dialog');

        // 阻止 dialog 内点击冒泡到 mask
        dialog.addEventListener('click', e => e.stopPropagation());

        // mask 点击关闭 modal
        this.modal.querySelector('.modbus-mask').addEventListener('click', () => this.closeModal());

        // 保存按钮
        this.modal.querySelector('#m-ok').addEventListener('click', () => this.saveModal());

        // 取消按钮
        this.modal.querySelector('#m-cancel').addEventListener('click', () => this.closeModal());
    }

    openModal(data) {
        this.editIndex = this.editIndex ?? null;

        // 填充表单
        this.modal.querySelector('#m-address').value = data.address;
        this.modal.querySelector('#m-name').value = data.name;
        this.modal.querySelector('#m-type').value = data.type;
        this.modal.querySelector('#m-length').value = data.length || 1;
        this.modal.querySelector('#m-scale').value = data.scale ?? '';
        this.modal.querySelector('#m-byte').value = data.byte_order || '';
        this.modal.querySelector('#m-desc').value = data.description || '';

        // **禁止背景滚动**
        document.body.style.overflow = 'hidden';

        // **显示 modal**
        this.modal.classList.remove('hidden');
    }

    closeModal() {
        // **恢复背景滚动**
        document.body.style.overflow = '';

        // 隐藏 modal
        this.modal.classList.add('hidden');
    }

    saveModal() {
        const r = {
            address: Number(this.modal.querySelector('#m-address').value),
            name: this.modal.querySelector('#m-name').value,
            type: this.modal.querySelector('#m-type').value,
            length: Number(this.modal.querySelector('#m-length').value),
            scale: this.modal.querySelector('#m-scale').value === ''
                ? undefined
                : Number(this.modal.querySelector('#m-scale').value),
            byte_order: this.modal.querySelector('#m-byte').value,
            description: this.modal.querySelector('#m-desc').value
        };

        if (this.editIndex === null) {
            this.config.registers.push(r);
        } else {
            this.config.registers[this.editIndex] = r;
        }

        this.closeModal();
        this.render();
    }
}

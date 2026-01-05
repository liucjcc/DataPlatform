export class ModbusRegisterConfigurator {
    constructor(containerId, protocolConfig) {
        this.container = document.getElementById(containerId);
        if (!this.container) throw new Error(`Container #${containerId} not found`);

        this.protocol = protocolConfig;
        this.registers = [...(protocolConfig.registers || [])];

        this.init();
    }

    exportConfig() {
        return {
            ...this.protocol,
            registers: this.getRegisters()
        };
    }

    init() {
        // 👇 先创建弹窗和遮罩（只创建一次！）
        this.createModal();
        // 再渲染主界面
        this.renderMain();
        // 绑定事件（包括弹窗内的按钮）
        this.bindEvents();
    }

    // 👇 仅渲染主区域（表格 + 添加按钮）
    renderMain() {
        this.container.innerHTML = '';

        const table = document.createElement('table');
        table.id = 'registers-table';
        table.classList.add('data-table');
        table.innerHTML = `
      <thead>
        <tr>
          <th>名称</th>
          <th>地址</th>
          <th>类型</th>
          <th>缩放系数</th>
          <th>字节序</th>
          <th>长度(寄存器数)</th>
          <th>描述</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody></tbody>
    `;
        const tbody = table.querySelector('tbody');

        this.registers.forEach((reg, index) => {
            const row = document.createElement('tr');
            row.dataset.index = index;
            row.innerHTML = `
        <td>${this.escapeHtml(reg.name)}</td>
        <td>${reg.address}</td>
        <td>${reg.type}</td>
        <td>${reg.scale !== undefined ? reg.scale : ''}</td>
        <td>${reg.byte_order || ''}</td>
        <td>${reg.length || (this.getTypeSize(reg.type) / 2) || ''}</td>
        <td>${this.escapeHtml(reg.description || '')}</td>
        <td>
          <button class="edit-btn">编辑</button>
          <button class="delete-btn">删除</button>
        </td>
      `;
            tbody.appendChild(row);
        });

        const buttonBar = document.createElement('div');
        buttonBar.style.marginTop = '10px';
        buttonBar.innerHTML = `<button id="add-register-btn">添加寄存器</button><button id="print-config-btn">添加寄存器</button>`;

        this.container.appendChild(table);
        this.container.appendChild(buttonBar);
    }

    // 👇 弹窗和遮罩只创建一次
    createModal() {
        // 遮罩
        this.overlay = document.createElement('div');
        this.overlay.id = 'modal-overlay';
        this.overlay.style.cssText = `
      display: none;
      position: fixed;
      top: 0; left: 0;
      width: 100%; height: 100%;
      background: rgba(0,0,0,0.5);
      z-index: 1000;
    `;

        // 弹窗
        this.modal = document.createElement('div');
        this.modal.id = 'register-modal';
        this.modal.style.cssText = `
      display: none;
      position: fixed;
      top: 50%; left: 50%;
      transform: translate(-50%, -50%);
      background: white;
      padding: 20px;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.3);
      z-index: 1001;
      width: 90%;
      max-width: 500px;
    `;
        this.modal.innerHTML = `
      <h3 id="modal-title">添加寄存器</h3>
      <form id="register-form">
        <input type="hidden" id="edit-index" value="-1">
        <div><label>名称: <input required id="name" name="name"></label></div><br>
        <div><label>地址: <input type="number" required min="0" id="address" name="address"></label></div><br>
        <div><label>类型:
          <select id="type" name="type" required>
            ${Object.keys(this.protocol.data_types).map(t => `<option value="${t}">${t}</option>`).join('')}
            <option value="int32">int32</option>
            <option value="uint32">uint32</option>
            <option value="int64">int64</option>
            <option value="uint64">uint64</option>
          </select>
        </label></div><br>
        <div><label>缩放系数: <input type="number" step="any" id="scale" name="scale"></label></div><br>
        <div><label>字节序:
          <select id="byte_order" name="byte_order">
            <option value="ABCD">ABCD</option>
            <option value="BADC">BADC</option>
            <option value="CDAB">CDAB</option>
            <option value="DCBA">DCBA</option>
          </select>
        </label></div><br>
        <div><label>长度 (寄存器数): <input type="number" min="1" id="length" name="length"></label></div><br>
        <div><label>描述: <textarea id="description" name="description" rows="2" style="width:100%"></textarea></label></div><br>
        <div>
          <button type="submit">保存</button>
          <button type="button" id="cancel-modal">取消</button>
        </div>
      </form>
    `;

        document.body.appendChild(this.overlay);
        document.body.appendChild(this.modal);
    }

    bindEvents() {
        const container = this.container;
        const modal = this.modal;
        const overlay = this.overlay;
        const form = modal.querySelector('#register-form');

        // 主界面事件（使用事件委托，安全）
        container.addEventListener('click', (e) => {
            if (e.target.id === 'add-register-btn') {
                this.openModal();
            } else if (e.target.classList.contains('edit-btn')) {
                const index = parseInt(e.target.closest('tr').dataset.index);
                this.openModal(index);
            } else if (e.target.classList.contains('delete-btn')) {
                if (confirm('确定删除该寄存器？')) {
                    const index = parseInt(e.target.closest('tr').dataset.index);
                    this.registers.splice(index, 1);
                    this.renderMain(); // 👈 只重绘主区域
                }
            } else if (e.target.id == 'print-config-btn') {

            }
        });

        // 弹窗内表单提交
        form.addEventListener('submit', (e) => {
            e.preventDefault();
            const formData = new FormData(form);
            const index = parseInt(modal.querySelector('#edit-index').value);

            const reg = {
                name: formData.get('name').trim(),
                address: parseInt(formData.get('address')),
                type: formData.get('type'),
                scale: formData.get('scale') ? parseFloat(formData.get('scale')) : undefined,
                byte_order: formData.get('byte_order') || undefined,
                length: formData.get('length') ? parseInt(formData.get('length')) : undefined,
                description: formData.get('description').trim() || undefined
            };

            Object.keys(reg).forEach(k => reg[k] === undefined && delete reg[k]);

            if (index >= 0) {
                this.registers[index] = reg;
            } else {
                this.registers.push(reg);
            }

            this.closeModal();
            this.renderMain(); // 👈 更新表格
        });

        // 取消按钮（只绑定一次，永久有效）
        modal.querySelector('#cancel-modal').addEventListener('click', () => {
            this.closeModal();
        });

        // 点击遮罩关闭
        overlay.addEventListener('click', () => {
            this.closeModal();
        });

        // ESC 关闭
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && modal.style.display === 'block') {
                this.closeModal();
            }
        });
    }

    openModal(editIndex = -1) {
        const modal = this.modal;
        const form = modal.querySelector('#register-form');

        form.reset();
        modal.querySelector('#edit-index').value = editIndex;

        if (editIndex >= 0) {
            const reg = this.registers[editIndex];
            modal.querySelector('#modal-title').textContent = '编辑寄存器';
            modal.querySelector('#name').value = reg.name || '';
            modal.querySelector('#address').value = reg.address || 0;
            modal.querySelector('#type').value = reg.type || 'int16';
            modal.querySelector('#scale').value = reg.scale || '';
            modal.querySelector('#byte_order').value = reg.byte_order || 'ABCD';
            modal.querySelector('#length').value = reg.length || '';
            modal.querySelector('#description').value = reg.description || '';
        } else {
            modal.querySelector('#modal-title').textContent = '添加寄存器';
        }

        this.overlay.style.display = 'block';
        this.modal.style.display = 'block';
        modal.querySelector('#name').focus();
    }

    closeModal() {
        this.overlay.style.display = 'none';
        this.modal.style.display = 'none';
    }

    getTypeSize(type) {
        const dt = this.protocol.data_types[type];
        if (dt) return dt.size;
        if (type.includes('64')) return 8;
        if (type.includes('32')) return 4;
        if (type.includes('16')) return 2;
        return 1;
    }

    escapeHtml(str) {
        if (typeof str !== 'string') return str;
        return str
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    getRegisters() {
        return [...this.registers];
    }

    addRegister() {
        this.openModal();
    }
}

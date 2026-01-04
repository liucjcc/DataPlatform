// comboBoxComponent.js
/**
 * 创建一个简单的 ComboBox 组件
 * @param {Object} options
 * @param {string} options.containerId - 容器 DOM id
 * @param {Array<{value: string, text: string}>} options.items - 下拉选项
 * @param {function} options.onChange - 选中回调
 * @returns {Object} { select, destroy }
 */
export function createComboBox({ containerId, items = [], onChange }) {
    const container = document.getElementById(containerId);
    if (!container) return null;

    // 创建 select DOM
    const wrapper = document.createElement('div');
    wrapper.className = 'select-wrapper';
    const optionsHtml = items.map(i => `<option value="${i.value}">${i.text}</option>`).join('');
    wrapper.innerHTML = `<select class="custom-select">${optionsHtml}</select>`;
    container.appendChild(wrapper);

    const select = wrapper.querySelector('select');

    // 事件回调
    const handler = (e) => {
        onChange && onChange(e.target.value);
    };
    select.addEventListener('change', handler);

    // 返回销毁函数
    const destroy = () => {
        select.removeEventListener('change', handler);
        container.removeChild(wrapper);
    };

    return { select, destroy };
}
